using CSharpDoctor.Core.Configuration;
using CSharpDoctor.Core.Inspection;
using CSharpDoctor.Core.Workspace;
using CSharpDoctor.Cli.IntegrationTests.Fixtures;
using CSharpDoctor.Wiring;
using Shouldly;
using Xunit;

namespace CSharpDoctor.Cli.IntegrationTests;

[Collection("workspace")]
public sealed class WiringInspectorTests
{
    [Fact]
    public async Task Run_WithoutFlag_DoesNotEmitCsdw0001()
    {
        using var fixture = new HostedServiceFixture();

        var result = await RunAsync(fixture.Root, wiring: false);

        result.Diagnostics.ShouldNotContain(d => d.Rule == "CSDW0001");
    }

    [Fact]
    public async Task Run_WithWiringFlag_EmitsCsdw0001_ForUnregisteredWorker()
    {
        using var fixture = new HostedServiceFixture();

        var result = await RunAsync(fixture.Root, wiring: true);

        var diagnostic = result.Diagnostics.Single(d => d.Rule == "CSDW0001");
        diagnostic.Message.ShouldContain("Worker");
        diagnostic.FilePath.ShouldBe(fixture.WorkerPath);
    }

    [Fact]
    public async Task Run_WithWiringFlag_DoesNotEmit_WhenRegistrationExists()
    {
        using var fixture = new HostedServiceFixture();
        fixture.WriteRegisteredProgram();

        var result = await RunAsync(fixture.Root, wiring: true);

        result.Diagnostics.ShouldNotContain(d => d.Rule == "CSDW0001");
    }

    [Fact]
    public async Task Run_WithWiringFlag_EmitsCsdw0008_ForUnresolvedInterface()
    {
        using var fixture = new DiGraphFixture();

        var result = await RunAsync(fixture.Root, wiring: true);

        result.Diagnostics.ShouldContain(d => d.Rule == "CSDW0008");
        result.Diagnostics.ShouldContain(d => d.FilePath == fixture.OrderServicePath);
    }

    [Fact]
    public async Task Run_WithWiringFlag_DoesNotEmitCsdw0008_WhenInterfaceRegistered()
    {
        using var fixture = new DiGraphFixture();
        fixture.WriteRegisteredProgram();

        var result = await RunAsync(fixture.Root, wiring: true);

        result.Diagnostics.ShouldNotContain(d => d.Rule == "CSDW0008");
    }

    [Fact]
    public async Task Run_WithWiringFlag_DoesNotEmitCsdw0008_ForFrameworkProvidedServices()
    {
        using var fixture = new DiGraphFixture();
        fixture.WriteFrameworkServiceProgram();

        var result = await RunAsync(fixture.Root, wiring: true);

        result.Diagnostics.ShouldNotContain(d => d.Rule == "CSDW0008");
    }

    [Fact]
    public async Task Run_WithWiringFlag_EmitsCsdw0010_ForCaptiveDependency()
    {
        using var fixture = new DiGraphFixture();
        fixture.WriteCaptiveDependencyProgram();

        var result = await RunAsync(fixture.Root, wiring: true);

        result.Diagnostics.ShouldContain(d => d.Rule == "CSDW0010");
    }

    private static async Task<InspectResult> RunAsync(string root, bool wiring)
    {
        using var provider = new MSBuildWorkspaceProvider();
        var externals = new Dictionary<string, Func<IInspector>>
        {
            [InspectorRegistry.WiringInspectorName] = () => new WiringInspector(),
        };
        var orchestrator = new InspectOrchestrator(() => [], externals, provider.LoadAsync);

        return await orchestrator.RunAsync(
            new InspectRequest(
                root,
                InspectMode.Full,
                CliOverrides: new CliConfigOverrides(Lint: false, Wiring: wiring)));
    }
}

[CollectionDefinition("workspace", DisableParallelization = true)]
public sealed class WorkspaceCollection;
