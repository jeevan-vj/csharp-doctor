using CSharpDoctor.Analyzers;
using CSharpDoctor.Core.Configuration;
using CSharpDoctor.Core.Inspection;
using CSharpDoctor.Core.Pipeline;
using CSharpDoctor.Core.Reporting;
using CSharpDoctor.Core.Workspace;
using CSharpDoctor.Architecture;
using CSharpDoctor.Ef;
using CSharpDoctor.Sca;
using CSharpDoctor.Wiring;
using System.CommandLine;
using System.CommandLine.Invocation;

namespace CSharpDoctor.Cli;

/// <summary>Builds and runs the csharp-doctor command. Extracted so output can be captured in tests.</summary>
public static class CliRunner
{
    internal const string DefaultHtmlReportFileName = "csharp-doctor-report.html";

    public static Task<int> RunAsync(string[] args, TextWriter output, TextWriter error)
    {
        var targetArgument = new Argument<string?>("path", "Directory, solution, or project to scan")
        {
            Arity = ArgumentArity.ZeroOrOne,
        };

        var diffOption = new Option<string?>("--diff", "Scan only files changed against base ref");
        var stagedOption = new Option<bool>("--staged", "Scan only staged files");
        var projectOption = new Option<string?>("--project", "Specific project or solution path");
        var failOnOption = new Option<FailOnLevel>("--fail-on", () => FailOnLevel.Error, "Exit code gate");
        var formatOption = new Option<OutputFormat>("--format", () => OutputFormat.Console, "Output format (console, json, sarif, html)");
        var outputOption = new Option<string?>("--output", "Report file path (default with --format html: <scan-path>/csharp-doctor-report.html)");
        var jsonOption = new Option<bool>("--json", "Emit JSON report (alias for --format json)");
        var annotationsOption = new Option<bool>("--annotations", "Emit GitHub Actions annotations");
        var noLintOption = new Option<bool>("--no-lint", "Skip Roslyn lint analyzers");
        var wiringOption = new Option<bool>("--wiring", "Enable wiring checks (unregistered services/handlers; whole solution)");
        var noWiringOption = new Option<bool>("--no-wiring", "Disable wiring checks");
        var efOption = new Option<bool>("--ef", "Enable EF Core migration safety checks");
        var noEfOption = new Option<bool>("--no-ef", "Disable EF Core migration safety checks");
        var architectureOption = new Option<bool>("--architecture", "Enable architecture checks (namespace cycles, dead code; whole solution)");
        var noArchitectureOption = new Option<bool>("--no-architecture", "Disable architecture checks");
        var scaOption = new Option<bool>("--sca", "Enable NuGet vulnerability scan (direct and transitive)");
        var noScaOption = new Option<bool>("--no-sca", "Disable NuGet vulnerability scan");
        var allChecksOption = new Option<bool>("--all-checks", "Enable every inspector (lint + wiring + ef + architecture + sca)");

        var rootCommand = new RootCommand(
            """
            Scan .NET codebases for security, correctness, and performance issues.

            SCAN MODES
              Default: lint only (Roslyn analyzers).
              --wiring      also detect unregistered services/handlers/validators (slower; whole solution).
              --ef          also detect unsafe EF Core migrations.
              --architecture also detect namespace cycles and dead-code heuristics.
              --sca         also scan NuGet packages for known vulnerabilities.
              --all-checks  enable everything.
            """)
        {
            targetArgument,
            diffOption,
            stagedOption,
            projectOption,
            failOnOption,
            formatOption,
            outputOption,
            jsonOption,
            annotationsOption,
            noLintOption,
            wiringOption,
            noWiringOption,
            efOption,
            noEfOption,
            architectureOption,
            noArchitectureOption,
            scaOption,
            noScaOption,
            allChecksOption,
        };

        rootCommand.SetHandler(async (InvocationContext context) =>
        {
            var parseResult = context.ParseResult;
            var path = parseResult.GetValueForArgument(targetArgument) ?? Directory.GetCurrentDirectory();
            var project = parseResult.GetValueForOption(projectOption);
            var targetDirectory = project ?? path;

            var staged = parseResult.GetValueForOption(stagedOption);
            var diff = parseResult.GetValueForOption(diffOption);

            if (staged && diff is not null)
            {
                error.WriteLine("Cannot combine --staged and --diff.");
                context.ExitCode = 2;
                return;
            }

            if (parseResult.GetValueForOption(jsonOption) && parseResult.GetValueForOption(formatOption) != OutputFormat.Console)
            {
                error.WriteLine("Cannot combine --json with --format.");
                context.ExitCode = 2;
                return;
            }

            var checks = CliCheckResolver.Resolve(
                noLint: parseResult.GetValueForOption(noLintOption),
                wiring: parseResult.GetValueForOption(wiringOption),
                noWiring: parseResult.GetValueForOption(noWiringOption),
                ef: parseResult.GetValueForOption(efOption),
                noEf: parseResult.GetValueForOption(noEfOption),
                architecture: parseResult.GetValueForOption(architectureOption),
                noArchitecture: parseResult.GetValueForOption(noArchitectureOption),
                sca: parseResult.GetValueForOption(scaOption),
                noSca: parseResult.GetValueForOption(noScaOption),
                allChecks: parseResult.GetValueForOption(allChecksOption),
                staged: staged);

            if (checks.ConflictError is not null)
            {
                error.WriteLine(checks.ConflictError);
                context.ExitCode = 2;
                return;
            }

            foreach (var warning in checks.Warnings)
            {
                error.WriteLine(warning);
            }

            var mode = staged ? InspectMode.Staged
                : diff is not null ? InspectMode.Diff
                : InspectMode.Full;

            var outputFormat = parseResult.GetValueForOption(jsonOption)
                ? OutputFormat.Json
                : parseResult.GetValueForOption(formatOption);

            var cliOverrides = new CliConfigOverrides(
                Lint: checks.Lint,
                Wiring: checks.Wiring,
                Ef: checks.Ef,
                Architecture: checks.Architecture,
                Sca: checks.Sca,
                FailOn: parseResult.GetValueForOption(failOnOption));

            var config = ConfigResolver.Resolve(targetDirectory, cliOverrides);

            if (!config.Checks.Lint && !config.Checks.Wiring && !config.Checks.Ef && !config.Checks.Architecture && !config.Checks.Sca)
            {
                error.WriteLine("Nothing to scan: all checks are disabled. Remove --no-lint or pass --wiring/--ef/--architecture/--sca.");
                context.ExitCode = 2;
                return;
            }

            var request = new InspectRequest(
                TargetDirectory: targetDirectory,
                Mode: mode,
                DiffBase: diff,
                CliOverrides: cliOverrides,
                OutputFormat: outputFormat,
                GitHubAnnotations: parseResult.GetValueForOption(annotationsOption));

            using var workspaceProvider = new MSBuildWorkspaceProvider(error);
            var externalInspectors = new Dictionary<string, Func<IInspector>>
            {
                [InspectorRegistry.WiringInspectorName] = () => new WiringInspector(),
                [InspectorRegistry.EfInspectorName] = () => new EfMigrationInspector(),
                [InspectorRegistry.ArchitectureInspectorName] = () => new ArchitectureInspector(),
                [InspectorRegistry.ScaInspectorName] = () => new ScaInspector(),
            };

            var orchestrator = new InspectOrchestrator(
                AnalyzerCatalog.CreateAll,
                externalInspectors,
                workspaceProvider.LoadAsync);
            var result = await orchestrator.RunAsync(request, context.GetCancellationToken()).ConfigureAwait(false);
            var rendered = orchestrator.RenderOutput(result, request, config);
            var outputPath = parseResult.GetValueForOption(outputOption);

            if (outputFormat == OutputFormat.Html)
            {
                var reportPath = string.IsNullOrWhiteSpace(outputPath)
                    ? Path.Combine(targetDirectory, DefaultHtmlReportFileName)
                    : outputPath;
                var fullPath = Path.GetFullPath(reportPath);
                var directory = Path.GetDirectoryName(fullPath);
                if (!string.IsNullOrWhiteSpace(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                await File.WriteAllTextAsync(fullPath, rendered, context.GetCancellationToken()).ConfigureAwait(false);
                output.WriteLine($"HTML report written to {fullPath}");
            }
            else if (outputFormat == OutputFormat.Console)
            {
                output.WriteLine(rendered);
            }
            else
            {
                await output.WriteAsync(rendered).ConfigureAwait(false);
            }

            if (request.GitHubAnnotations)
            {
                var forPr = DiagnosticSurfaceFilter.FilterForSurface(
                    result.Diagnostics,
                    OutputSurface.PrComment,
                    config);
                await output.WriteAsync(GitHubAnnotationWriter.Render(forPr)).ConfigureAwait(false);
            }

            context.ExitCode = result.ExitCode;
        });

        return rootCommand.InvokeAsync(args);
    }
}
