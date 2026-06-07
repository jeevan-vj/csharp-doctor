using CSharpDoctor.Core.Configuration;
using CSharpDoctor.Core.Inspection;
using CSharpDoctor.Cli.IntegrationTests.Fixtures;
using CSharpDoctor.Ef;
using Shouldly;
using Xunit;

namespace CSharpDoctor.Cli.IntegrationTests;

public sealed class EfInspectorTests
{
    [Fact]
    public async Task Run_WithoutFlag_DoesNotEmitCsde0001()
    {
        using var fixture = new EfMigrationFixture();

        var result = await RunAsync(fixture.Root, ef: false);

        result.Diagnostics.ShouldNotContain(d => d.Rule == "CSDE0001");
    }

    [Fact]
    public async Task Run_WithEfFlag_EmitsCsde0001_PointingToDropColumnCall()
    {
        using var fixture = new EfMigrationFixture();

        var result = await RunAsync(fixture.Root, ef: true);

        var diagnostic = result.Diagnostics.ShouldHaveSingleItem();
        diagnostic.Rule.ShouldBe("CSDE0001");
        diagnostic.Message.ShouldContain("OldCol");
        diagnostic.Message.ShouldContain("Users");
        diagnostic.FilePath.ShouldBe(fixture.MigrationPath);
    }

    [Fact]
    public async Task Run_WithEfFlag_OnProjectWithoutMigrations_EmitsNothing()
    {
        using var fixture = new EfMigrationFixture(includeMigration: false);

        var result = await RunAsync(fixture.Root, ef: true);

        result.Diagnostics.ShouldNotContain(d => d.Rule == "CSDE0001");
    }

    [Fact]
    public async Task Run_WithEfFlag_EmitsCsde0002_ForNotNullColumnWithoutDefault()
    {
        using var fixture = new EfAddColumnFixture();

        var result = await RunAsync(fixture.Root, ef: true);

        result.Diagnostics.ShouldContain(d => d.Rule == "CSDE0002");
    }

    [Fact]
    public async Task Run_WithEfFlag_DoesNotEmitCsde0002_WhenDefaultProvided()
    {
        using var fixture = new EfAddColumnFixture(withDefault: true);

        var result = await RunAsync(fixture.Root, ef: true);

        result.Diagnostics.ShouldNotContain(d => d.Rule == "CSDE0002");
    }

    [Fact]
    public async Task Run_WithEfFlag_DoesNotEmitCsde0002_WhenColumnIsNullable()
    {
        using var fixture = new EfAddColumnFixture(nullable: true);

        var result = await RunAsync(fixture.Root, ef: true);

        result.Diagnostics.ShouldNotContain(d => d.Rule == "CSDE0002");
    }

    [Fact]
    public async Task Run_WithEfFlag_EmitsCsde0006_ForTypeShrink()
    {
        using var fixture = new EfAlterColumnFixture(shrink: true);

        var result = await RunAsync(fixture.Root, ef: true);

        result.Diagnostics.ShouldContain(d => d.Rule == "CSDE0006");
    }

    [Fact]
    public async Task Run_WithEfFlag_DoesNotEmitCsde0006_ForTypeWiden()
    {
        using var fixture = new EfAlterColumnFixture(shrink: false);

        var result = await RunAsync(fixture.Root, ef: true);

        result.Diagnostics.ShouldNotContain(d => d.Rule == "CSDE0006");
    }

    private static async Task<InspectResult> RunAsync(string root, bool ef)
    {
        var externals = new Dictionary<string, Func<IInspector>>
        {
            [InspectorRegistry.EfInspectorName] = () => new EfMigrationInspector(),
        };
        var orchestrator = new InspectOrchestrator(() => [], externals);

        return await orchestrator.RunAsync(
            new InspectRequest(
                root,
                InspectMode.Full,
                CliOverrides: new CliConfigOverrides(Lint: false, Ef: ef)));
    }
}
