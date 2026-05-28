using Shouldly;
using Xunit;

namespace CSharpDoctor.Architecture.Tests;

public sealed class TestingBaselineTests
{
    [Fact]
    public void Harness_DiscoversTests_AndRunsShouldly()
    {
        (1 + 1).ShouldBe(2);
    }
}
