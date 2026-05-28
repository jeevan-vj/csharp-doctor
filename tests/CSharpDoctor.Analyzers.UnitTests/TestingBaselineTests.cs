using Shouldly;
using Xunit;

namespace CSharpDoctor.Analyzers.UnitTests;

public sealed class TestingBaselineTests
{
    [Fact]
    public void Harness_DiscoversTests_AndRunsShouldly()
    {
        (1 + 1).ShouldBe(2);
    }
}
