using Shouldly;
using Xunit;

namespace CSharpDoctor.Core.UnitTests;

/// <summary>Proves xUnit + Shouldly discovery. Replaced by feature tests as phases land.</summary>
public sealed class TestingBaselineTests
{
    [Fact]
    public void Harness_DiscoversTests_AndRunsShouldly()
    {
        (1 + 1).ShouldBe(2);
    }
}
