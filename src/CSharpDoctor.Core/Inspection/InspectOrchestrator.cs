using System.Collections.Immutable;
using System.Diagnostics;
using CSharpDoctor.Core.Analysis;
using CSharpDoctor.Core.Configuration;
using CSharpDoctor.Core.Diagnostics;
using CSharpDoctor.Core.Discovery;
using CSharpDoctor.Core.Pipeline;
using CSharpDoctor.Core.Reporting;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpDoctor.Core.Inspection;

/// <summary>Coordinates discovery, analysis, pipeline, and reporting.</summary>
public sealed class InspectOrchestrator
{
    private readonly Func<ImmutableArray<DiagnosticAnalyzer>> _analyzerFactory;

    public InspectOrchestrator(Func<ImmutableArray<DiagnosticAnalyzer>>? analyzerFactory = null) =>
        _analyzerFactory = analyzerFactory ?? (() => []);

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

        var projects = ProjectDiscovery.FindProjects(request.TargetDirectory);
        var scanRoot = projects.FirstOrDefault() is not null
            ? Path.GetDirectoryName(projects[0]) ?? request.TargetDirectory
            : request.TargetDirectory;

        var sourceFiles = ProjectDiscovery.FindSourceFiles(scanRoot, includeFiles);
        var csprojPaths = ProjectDiscovery.FindCsprojFiles(request.TargetDirectory);
        var fileCapabilities = ProjectFileCapabilities.Build(csprojPaths, sourceFiles);
        var raw = await AnalyzerRunner.RunAsync(sourceFiles, _analyzerFactory(), cancellationToken)
            .ConfigureAwait(false);

        var readLines = new Func<string, IReadOnlyList<string>>(path =>
            File.Exists(path) ? File.ReadAllLines(path) : Array.Empty<string>());

        var pipeline = DiagnosticPipelineFactory.CreateDefault(config, readLines, fileCapabilities);
        var processed = pipeline.Apply(raw);

        var forGate = DiagnosticSurfaceFilter.FilterForSurface(processed, OutputSurface.CiFailure, config);
        var exitCode = ComputeExitCode(forGate, config.FailOn);

        stopwatch.Stop();
        return new InspectResult(
            Diagnostics: processed,
            ErrorCount: processed.Count(d => d.Severity == DiagnosticSeverity.Error),
            WarningCount: processed.Count(d => d.Severity == DiagnosticSeverity.Warning),
            Elapsed: stopwatch.Elapsed,
            ExitCode: exitCode);
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
            OutputFormat.Sarif => SarifReportBuilder.Build(result.Diagnostics),
            _ => ConsoleReporter.Render(
                DiagnosticSurfaceFilter.FilterForSurface(result.Diagnostics, OutputSurface.Cli, config),
                result.Elapsed),
        };
}
