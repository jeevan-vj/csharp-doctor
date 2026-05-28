using CSharpDoctor.Analyzers.Rules.Security;
using Xunit;

namespace CSharpDoctor.Analyzers.UnitTests.Rules.Security;

public sealed class Csd0004Tests
{
    [Fact]
    public async Task ShouldReport_WhenConnectionStringContainsPassword()
    {
        await AnalyzerTest<Csd0004HardcodedSecretAnalyzer>.RunAsync(
            """
            class C {
                string M() => "Server=.;Password=SuperSecret123;";
            }
            """,
            "CSD0004");
    }

    [Fact]
    public async Task ShouldNotReport_WhenNoSecretPattern()
    {
        await AnalyzerTest<Csd0004HardcodedSecretAnalyzer>.RunAsync(
            """
            class C { string M() => "Hello world"; }
            """);
    }
}
