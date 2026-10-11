using System.Collections.Immutable;
using System.Diagnostics;
using CSharpDoctor.Core.Configuration;
using CSharpDoctor.Core.Diagnostics;
using CSharpDoctor.Core.Discovery;
using CSharpDoctor.Core.Inspectors;
using CSharpDoctor.Core.Pipeline;
using CSharpDoctor.Core.Reporting;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using DiagnosticSeverity = CSharpDoctor.Core.Diagnostics.DiagnosticSeverity;

namespace CSharpDoctor.Core.Inspection;

/// <summary>Coordinates discovery, analysis, pipeline, and reporting.</summary>
public sealed class InspectOrchestrator
{
    private readonly Func<ImmutableArray<DiagnosticAnalyzer>> _analyzerFactory;
    private readonly IReadOnlyDictionary<string, Func<IInspector>>? _externalInspectorFactories;
    private readonly Func<string, CancellationToken, Task<Solution?>>? _solutionLoader;

    public InspectOrchestrator(
        Func<ImmutableArray<DiagnosticAnalyzer>>? analyzerFactory = null,
        IReadOnlyDictionary<string, Func<IInspector>>? externalInspectorFactories = null,
        Func<string, CancellationToken, Task<Solution?>>? solutionLoader = null)
    {
        _analyzerFactory = analyzerFactory ?? (() => []);
        _externalInspectorFactories = externalInspectorFactories;
        _solutionLoader = solutionLoader;
    }

    public async Task<InspectResult> RunAsync(
        InspectRequest request,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var config = ConfigResolver.Resolve(request.TargetDirectory, request.CliOverrides, cancellationToken);

        var includeFiles = request.Mode switch
        {
            InspectMode.Diff => GitScopeResolver.ResolveIncludePaths(
                request.TargetDirectory,
                InspectMode.Diff,
                request.DiffBase),
            InspectMode.Staged => GitScopeResolver.ResolveIncludePaths(
                request.TargetDirectory,
                InspectMode.Staged,
                null),
            _ => request.IncludeFiles,
        };

        var scanRoot = ProjectDiscovery.ResolveScanRoot(request.TargetDirectory);
        var sourceFiles = ProjectDiscovery.FindSourceFiles(scanRoot, includeFiles);
        var csprojPaths = ProjectDiscovery.FindCsprojFiles(request.TargetDirectory);
        var fileCapabilities = ProjectFileCapabilities.Build(csprojPaths, sourceFiles);

        var solutionLazy = new Lazy<Task<Solution?>>(() =>
            _solutionLoader is null
                ? Task.FromResult<Solution?>(null)
                : _solutionLoader(request.TargetDirectory, cancellationToken));

        var context = new InspectContext(
            request,
            config,
            sourceFiles,
            csprojPaths,
            scanRoot,
            fileCapabilities,
            solutionLazy);

        var inspectors = InspectorRegistry.Resolve(
            config,
            () => new RoslynLintInspector(_analyzerFactory()),
            _externalInspectorFactories);

        var raw = new List<DiagnosticRecord>();
        var runs = new Dictionary<string, InspectorRunInfo>(StringComparer.OrdinalIgnoreCase);
        foreach (var inspector in inspectors)
        {
            var inspectorStopwatch = Stopwatch.StartNew();
            var diagnostics = await inspector.InspectAsync(context, cancellationToken).ConfigureAwait(false);
            inspectorStopwatch.Stop();

            raw.AddRange(diagnostics);
            runs[inspector.Name] = new InspectorRunInfo(inspectorStopwatch.Elapsed, diagnostics.Count, Ran: true);
        }

        var readLines = new Func<string, IReadOnlyList<string>>(path =>
            File.Exists(path) ? File.ReadAllLines(path) : Array.Empty<string>());

        var pipeline = DiagnosticPipelineFactory.CreateDefault(config, readLines, fileCapabilities);
        var processed = pipeline.Apply(raw);

        var forGate = DiagnosticSurfaceFilter.FilterForSurface(processed, OutputSurface.CiFailure, config);
        var metrics = ScanMetricsCalculator.Compute(forGate, config.QualityGates.DebtWeights);
        var baselinePath = string.IsNullOrWhiteSpace(config.QualityGates.BaselinePath)
            ? null
            : ConfigResolver.ResolvePathRelativeToConfig(config.ConfigFileDirectory, config.QualityGates.BaselinePath);
        var baselineIdentities = BaselineLoader.LoadIdentities(baselinePath);
        var qualityGates = QualityGateEvaluator.Evaluate(forGate, metrics, config.QualityGates, baselineIdentities);
        var hotSpots = HotSpotRanker.Rank(metrics, config.HotSpots);

        var exitCode = ComputeExitCode(forGate, config.FailOn);
        if (qualityGates.Enabled && !qualityGates.Passed)
        {
            exitCode = exitCode == 0 ? 1 : exitCode;
        }

        stopwatch.Stop();
        return new InspectResult(
            Diagnostics: processed,
            ErrorCount: processed.Count(d => d.Severity == DiagnosticSeverity.Error),
            WarningCount: processed.Count(d => d.Severity == DiagnosticSeverity.Warning),
            Elapsed: stopwatch.Elapsed,
            ExitCode: exitCode,
            InspectorRuns: runs,
            Metrics: metrics,
            QualityGates: qualityGates,
            HotSpots: hotSpots);
    }

    public static int ComputeExitCode(IReadOnlyList<DiagnosticRecord> diagnostics, FailOnLevel failOn) =>
        failOn switch
        {
            FailOnLevel.None => 0,
            FailOnLevel.Warning when diagnostics.Any(static d =>
                d.Severity is DiagnosticSeverity.Error or DiagnosticSeverity.Warning) => 1,
            FailOnLevel.Error when diagnostics.Any(static d => d.Severity == DiagnosticSeverity.Error) => 1,
            _ => 0,
        };

    public string RenderOutput(InspectResult result, InspectRequest request, CSharpDoctorConfig config) =>
        request.OutputFormat switch
        {
            OutputFormat.Json => JsonReportBuilder.Build(result, request.Mode),
            OutputFormat.Html => HtmlReportBuilder.Build(result, request.Mode, config),
            OutputFormat.Sarif => SarifReportBuilder.Build(result.Diagnostics),
            _ => ConsoleReporter.Render(
                DiagnosticSurfaceFilter.FilterForSurface(result.Diagnostics, OutputSurface.Cli, config),
                result.Elapsed,
                result.InspectorRuns,
                result.Metrics,
                result.QualityGates,
                result.HotSpots),
        };
}
