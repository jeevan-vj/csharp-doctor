using CSharpDoctor.Analyzers.Rules.Security;
using Xunit;

namespace CSharpDoctor.Analyzers.UnitTests.Rules.Security;

public sealed class Csd0005Tests
{
    [Fact]
    public async Task ShouldReport_WhenRandomUsedForTokenGeneration()
    {
        await AnalyzerTest<Csd0005InsecureRandomAnalyzer>.RunAsync(
            """
            class C {
                string GenerateToken() {
                    var rng = new System.Random();
                    return rng.Next().ToString();
                }
            }
            """,
            "CSD0005");
    }

    [Fact]
    public async Task ShouldNotReport_WhenNotSecurityContext()
    {
        await AnalyzerTest<Csd0005InsecureRandomAnalyzer>.RunAsync(
            """
            class C {
                int M() => new System.Random().Next(1, 10);
            }
            """);
    }
}
