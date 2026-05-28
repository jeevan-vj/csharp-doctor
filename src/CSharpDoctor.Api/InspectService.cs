using CSharpDoctor.Analyzers;
using CSharpDoctor.Core.Configuration;
using CSharpDoctor.Core.Inspection;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpDoctor.Api;

/// <summary>Programmatic inspection API.</summary>
public sealed class InspectService
{
    private readonly InspectOrchestrator _orchestrator;

    public InspectService() =>
        _orchestrator = new InspectOrchestrator(() => AnalyzerCatalog.CreateAll());

    public Task<InspectResult> InspectAsync(
        string directory,
        InspectMode mode = InspectMode.Full,
        CliConfigOverrides? cliOverrides = null,
        CancellationToken cancellationToken = default) =>
        _orchestrator.RunAsync(
            new InspectRequest(directory, mode, CliOverrides: cliOverrides),
            cancellationToken);
}
