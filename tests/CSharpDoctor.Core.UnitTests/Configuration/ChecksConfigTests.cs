using CSharpDoctor.Core.Configuration;
using Shouldly;
using Xunit;

namespace CSharpDoctor.Core.UnitTests.Configuration;

public sealed class ChecksConfigTests : IDisposable
{
    private readonly string _root;

    public ChecksConfigTests()
    {
        _root = Path.Combine(Path.GetTempPath(), "csd-checks-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_root);
    }

    [Fact]
    public void Resolve_WithDefaults_LintOnWiringAndEfOff()
    {
        var config = ConfigResolver.Resolve(_root);

        config.Checks.Lint.ShouldBeTrue();
        config.Checks.Wiring.ShouldBeFalse();
        config.Checks.Ef.ShouldBeFalse();
        config.Checks.Sca.ShouldBeFalse();
    }

    [Fact]
    public void Resolve_WithChecksScaTrueInFile_EnablesSca()
    {
        WriteConfig("""{ "checks": { "sca": true } }""");

        var config = ConfigResolver.Resolve(_root);

        config.Checks.Sca.ShouldBeTrue();
    }

    [Fact]
    public void Resolve_WithChecksWiringTrueInFile_EnablesWiring()
    {
        WriteConfig("""{ "checks": { "wiring": true } }""");

        var config = ConfigResolver.Resolve(_root);

        config.Checks.Wiring.ShouldBeTrue();
    }

    [Fact]
    public void Resolve_CliNoWiring_OverridesConfigWiringTrue()
    {
        WriteConfig("""{ "checks": { "wiring": true } }""");

        var config = ConfigResolver.Resolve(_root, new CliConfigOverrides(Wiring: false));

        config.Checks.Wiring.ShouldBeFalse();
    }

    [Fact]
    public void Resolve_LegacyTopLevelLintFalse_DisablesLintCheck()
    {
        WriteConfig("""{ "lint": false }""");

        var config = ConfigResolver.Resolve(_root);

        config.Checks.Lint.ShouldBeFalse();
    }

    [Fact]
    public void Resolve_WithUnknownChecksKey_ThrowsHelpfulError()
    {
        WriteConfig("""{ "checks": { "wirring": true } }""");

        var ex = Should.Throw<InvalidOperationException>(() => ConfigResolver.Resolve(_root));

        ex.Message.ShouldContain("wiring");
    }

    private void WriteConfig(string json) =>
        File.WriteAllText(Path.Combine(_root, ConfigResolver.ConfigFileName), json);

    public void Dispose()
    {
        try
        {
            Directory.Delete(_root, recursive: true);
        }
        catch
        {
            // best-effort cleanup
        }
    }
}
