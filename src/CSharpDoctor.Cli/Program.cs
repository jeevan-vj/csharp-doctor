using System.Collections.Immutable;
using CSharpDoctor.Analyzers;
using CSharpDoctor.Core.Configuration;
using CSharpDoctor.Core.Inspection;
using CSharpDoctor.Core.Pipeline;
using CSharpDoctor.Core.Reporting;
using Microsoft.CodeAnalysis.Diagnostics;
using System.CommandLine;
using System.CommandLine.Invocation;

var targetArgument = new Argument<string?>("path", "Directory, solution, or project to scan")
{
    Arity = ArgumentArity.ZeroOrOne,
};

var diffOption = new Option<string?>("--diff", "Scan only files changed against base ref");
var stagedOption = new Option<bool>("--staged", "Scan only staged files");
var projectOption = new Option<string?>("--project", "Specific project or solution path");
var failOnOption = new Option<FailOnLevel>("--fail-on", () => FailOnLevel.Error, "Exit code gate");
var formatOption = new Option<OutputFormat>("--format", () => OutputFormat.Console, "Output format");
var jsonOption = new Option<bool>("--json", "Emit JSON report (alias for --format json)");
var annotationsOption = new Option<bool>("--annotations", "Emit GitHub Actions annotations");
var noLintOption = new Option<bool>("--no-lint", "Skip lint analyzers");

var rootCommand = new RootCommand("Scan .NET codebases for security, correctness, and performance issues")
{
    targetArgument,
    diffOption,
    stagedOption,
    projectOption,
    failOnOption,
    formatOption,
    jsonOption,
    annotationsOption,
    noLintOption,
};

rootCommand.SetHandler(async (InvocationContext context) =>
{
    var parseResult = context.ParseResult;
    var path = parseResult.GetValueForArgument(targetArgument) ?? Directory.GetCurrentDirectory();
    var project = parseResult.GetValueForOption(projectOption);
    var targetDirectory = project ?? path;

    if (parseResult.GetValueForOption(stagedOption) && parseResult.GetValueForOption(diffOption) is not null)
    {
        Console.Error.WriteLine("Cannot combine --staged and --diff.");
        context.ExitCode = 2;
        return;
    }

    if (parseResult.GetValueForOption(jsonOption) && parseResult.GetValueForOption(formatOption) != OutputFormat.Console)
    {
        Console.Error.WriteLine("Cannot combine --json with --format.");
        context.ExitCode = 2;
        return;
    }

    var mode = parseResult.GetValueForOption(stagedOption) ? InspectMode.Staged
        : parseResult.GetValueForOption(diffOption) is not null ? InspectMode.Diff
        : InspectMode.Full;

    var outputFormat = parseResult.GetValueForOption(jsonOption)
        ? OutputFormat.Json
        : parseResult.GetValueForOption(formatOption);

    var cliOverrides = new CliConfigOverrides(
        FailOn: parseResult.GetValueForOption(failOnOption),
        Lint: parseResult.GetValueForOption(noLintOption) ? false : null);

    var request = new InspectRequest(
        TargetDirectory: targetDirectory,
        Mode: mode,
        DiffBase: parseResult.GetValueForOption(diffOption),
        CliOverrides: cliOverrides,
        OutputFormat: outputFormat,
        GitHubAnnotations: parseResult.GetValueForOption(annotationsOption));

    ImmutableArray<DiagnosticAnalyzer> analyzers = parseResult.GetValueForOption(noLintOption)
        ? []
        : AnalyzerCatalog.CreateAll();

    var orchestrator = new InspectOrchestrator(() => analyzers);
    var config = ConfigResolver.Resolve(targetDirectory, cliOverrides);
    var result = await orchestrator.RunAsync(request, context.GetCancellationToken()).ConfigureAwait(false);
    var output = orchestrator.RenderOutput(result, request, config);

    if (outputFormat == OutputFormat.Console)
    {
        Console.WriteLine(output);
    }
    else
    {
        await Console.Out.WriteAsync(output).ConfigureAwait(false);
    }

    if (request.GitHubAnnotations)
    {
        var forPr = DiagnosticSurfaceFilter.FilterForSurface(
            result.Diagnostics,
            OutputSurface.PrComment,
            config);
        await Console.Out.WriteAsync(GitHubAnnotationWriter.Render(forPr)).ConfigureAwait(false);
    }

    context.ExitCode = result.ExitCode;
});

return rootCommand.Invoke(args);
