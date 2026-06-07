using CSharpDoctor.Analyzers.Rules.Security;
using Xunit;

namespace CSharpDoctor.Analyzers.UnitTests.Rules.Security;

public sealed class Csd0012Tests
{
    [Fact]
    public async Task ShouldReport_WhenTls10Enabled()
    {
        await AnalyzerTest<Csd0012WeakTlsAnalyzer>.RunAsync(
            """
            class C {
                void M() {
                    System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls;
                }
            }
            """,
            "CSD0012");
    }

    [Fact]
    public async Task ShouldNotReport_WhenTls12Only()
    {
        await AnalyzerTest<Csd0012WeakTlsAnalyzer>.RunAsync(
            """
            class C {
                void M() {
                    System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
                }
            }
            """);
    }

    [Fact]
    public async Task ShouldNotReport_WhenPatternOnlyInStringLiteral()
    {
        await AnalyzerTest<Csd0012WeakTlsAnalyzer>.RunAsync(
            """
            class C {
                string M() => "SecurityProtocolType.Tls11 is weak";
            }
            """);
    }
}
