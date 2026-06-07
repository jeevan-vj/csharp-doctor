using CSharpDoctor.Cli;
using CSharpDoctor.Cli.IntegrationTests.Fixtures;
using Shouldly;
using Xunit;

namespace CSharpDoctor.Cli.IntegrationTests;

public sealed class CliFlagTests
{
    [Fact]
    public void Resolve_AllNegated_ProducesAllFalse()
    {
        var result = CliCheckResolver.Resolve(
            noLint: true, wiring: false, noWiring: true, ef: false, noEf: true,
            architecture: false, noArchitecture: true, sca: false, noSca: true,
            allChecks: false, staged: false);

        result.ConflictError.ShouldBeNull();
        result.Lint.ShouldBe(false);
        result.Wiring.ShouldBe(false);
        result.Ef.ShouldBe(false);
        result.Architecture.ShouldBe(false);
        result.Sca.ShouldBe(false);
    }

    [Fact]
    public void Resolve_WiringAndNoWiring_ReturnsConflict()
    {
        var result = CliCheckResolver.Resolve(
            noLint: false, wiring: true, noWiring: true, ef: false, noEf: false,
            architecture: false, noArchitecture: false, sca: false, noSca: false,
            allChecks: false, staged: false);

        result.ConflictError.ShouldNotBeNull();
        result.ConflictError!.ShouldContain("--wiring");
    }

    [Fact]
    public void Resolve_ScaAndNoSca_ReturnsConflict()
    {
        var result = CliCheckResolver.Resolve(
            noLint: false, wiring: false, noWiring: false, ef: false, noEf: false,
            architecture: false, noArchitecture: false, sca: true, noSca: true,
            allChecks: false, staged: false);

        result.ConflictError.ShouldNotBeNull();
        result.ConflictError!.ShouldContain("--sca");
    }

    [Fact]
    public void Resolve_WiringWithStaged_WarnsAndDisablesWiring()
    {
        var result = CliCheckResolver.Resolve(
            noLint: false, wiring: true, noWiring: false, ef: false, noEf: false,
            architecture: false, noArchitecture: false, sca: false, noSca: false,
            allChecks: false, staged: true);

        result.Wiring.ShouldBe(false);
        result.Warnings.ShouldContain(w => w.Contains("--staged"));
    }

    [Fact]
    public void Resolve_AllChecks_EnablesEverything()
    {
        var result = CliCheckResolver.Resolve(
            noLint: false, wiring: false, noWiring: false, ef: false, noEf: false,
            architecture: false, noArchitecture: false, sca: false, noSca: false,
            allChecks: true, staged: false);

        result.Lint.ShouldBe(true);
        result.Wiring.ShouldBe(true);
        result.Ef.ShouldBe(true);
        result.Architecture.ShouldBe(true);
        result.Sca.ShouldBe(true);
    }

    [Fact]
    public async Task Run_AllChecksDisabled_ExitsTwoWithNothingToScan()
    {
        using var fixture = new TempSolutionFixture();
        var output = new StringWriter();
        var error = new StringWriter();

        var exitCode = await CliRunner.RunAsync(
            ["--no-lint", "--no-wiring", "--no-ef", "--no-architecture", "--no-sca", fixture.Root],
            output,
            error);

        exitCode.ShouldBe(2);
        error.ToString().ShouldContain("Nothing to scan");
    }

    [Fact]
    public async Task Run_WiringWithStaged_WritesWarningToError()
    {
        using var fixture = new TempSolutionFixture();
        var output = new StringWriter();
        var error = new StringWriter();

        await CliRunner.RunAsync(["--wiring", "--staged", fixture.Root], output, error);

        error.ToString().ShouldContain("--staged");
    }
}
