using CSharpDoctor.Core.Configuration;
using Shouldly;
using Xunit;

namespace CSharpDoctor.Core.UnitTests.Configuration;

public sealed class QualityGatesConfigTests : IDisposable
{
    private readonly string _root;

    public QualityGatesConfigTests()
    {
        _root = Path.Combine(Path.GetTempPath(), "csd-gates-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_root);
    }

    [Fact]
    public void Resolve_LoadsQualityGatesFromConfigFile()
    {
        File.WriteAllText(
            Path.Combine(_root, ConfigResolver.ConfigFileName),
            """
            {
              "qualityGates": {
                "enabled": true,
                "maxErrors": 0,
                "maxDebtScore": 50
              },
              "hotSpots": {
                "topRules": 5
              }
            }
            """);

        var config = ConfigResolver.Resolve(_root);

        config.QualityGates.Enabled.ShouldBeTrue();
        config.QualityGates.MaxErrors.ShouldBe(0);
        config.QualityGates.MaxDebtScore.ShouldBe(50);
        config.HotSpots.TopRules.ShouldBe(5);
    }

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
