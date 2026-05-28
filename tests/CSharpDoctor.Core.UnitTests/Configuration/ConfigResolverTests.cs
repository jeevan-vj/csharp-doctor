using CSharpDoctor.Core.Configuration;
using Shouldly;
using Xunit;

namespace CSharpDoctor.Core.UnitTests.Configuration;

public sealed class ConfigResolverTests
{
    [Fact]
    public void PrecedenceIsCliOverFileOverDefaults()
    {
        var file = new CSharpDoctorConfig { FailOn = FailOnLevel.Warning, Lint = true };
        var merged = ConfigResolver.Merge(file, "/tmp");
        var result = ConfigResolver.ApplyCliOverrides(
            merged,
            new CliConfigOverrides(FailOn: FailOnLevel.None, Lint: false));

        result.FailOn.ShouldBe(FailOnLevel.None);
        result.Lint.ShouldBeFalse();
    }

    [Fact]
    public void RelativePathsResolveAgainstConfigFileDir()
    {
        ConfigResolver.ResolvePathRelativeToConfig("/repo/config", "../src")
            .ShouldBe(Path.GetFullPath(Path.Combine("/repo/config", "../src")));
    }
}
