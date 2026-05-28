using CSharpDoctor.Analyzers.Rules.Security;
using Xunit;

namespace CSharpDoctor.Analyzers.UnitTests.Rules.Security;

public sealed class Csd0014Tests
{
    [Fact]
    public async Task ShouldReport_WhenAttributeContainsPassword()
    {
        await AnalyzerTest<Csd0014SecretInAttributeAnalyzer>.RunAsync(
            """
            class C {
                [ConnectionString("Server=.;Password=Secret123")]
                void M() { }
            }
            class ConnectionStringAttribute : System.Attribute {
                public ConnectionStringAttribute(string value) { }
            }
            """,
            "CSD0014");
    }

    [Fact]
    public async Task ShouldNotReport_WhenAttributeHasNoSecret()
    {
        await AnalyzerTest<Csd0014SecretInAttributeAnalyzer>.RunAsync(
            """
            class C {
                [System.Obsolete("old")]
                void M() { }
            }
            """);
    }
}
