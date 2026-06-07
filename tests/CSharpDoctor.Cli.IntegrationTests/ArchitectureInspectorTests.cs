using CSharpDoctor.Architecture;
using CSharpDoctor.Core.Configuration;
using CSharpDoctor.Core.Inspection;
using CSharpDoctor.Core.Workspace;
using CSharpDoctor.Cli.IntegrationTests.Fixtures;
using Shouldly;
using Xunit;

namespace CSharpDoctor.Cli.IntegrationTests;

[Collection("workspace")]
public sealed class ArchitectureInspectorTests
{
    [Fact]
    public async Task Run_WithoutFlag_DoesNotEmitCsda0001()
    {
        using var fixture = new NamespaceCycleFixture();

        var result = await RunAsync(fixture.Root, architecture: false);

        result.Diagnostics.ShouldNotContain(d => d.Rule == "CSDA0001");
    }

    [Fact]
    public async Task Run_WithArchitectureFlag_EmitsCsda0001_ForNamespaceCycle()
    {
        using var fixture = new NamespaceCycleFixture();

        var result = await RunAsync(fixture.Root, architecture: true);

        result.Diagnostics.ShouldContain(d => d.Rule == "CSDA0001");
    }

    private static async Task<InspectResult> RunAsync(string root, bool architecture)
    {
        using var provider = new MSBuildWorkspaceProvider();
        var externals = new Dictionary<string, Func<IInspector>>
        {
            [InspectorRegistry.ArchitectureInspectorName] = () => new ArchitectureInspector(),
        };
        var orchestrator = new InspectOrchestrator(() => [], externals, provider.LoadAsync);

        return await orchestrator.RunAsync(
            new InspectRequest(
                root,
                InspectMode.Full,
                CliOverrides: new CliConfigOverrides(Lint: false, Architecture: architecture)));
    }
}
