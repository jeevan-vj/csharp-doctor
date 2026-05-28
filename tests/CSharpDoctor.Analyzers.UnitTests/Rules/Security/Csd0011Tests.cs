using CSharpDoctor.Analyzers.Rules.Security;
using Xunit;

namespace CSharpDoctor.Analyzers.UnitTests.Rules.Security;

public sealed class Csd0011Tests
{
    [Fact]
    public async Task ShouldReport_WhenCertValidationAlwaysTrue()
    {
        await AnalyzerTest<Csd0011HttpClientCertBypassAnalyzer>.RunAsync(
            """
            class C {
                void M() {
                    var handler = new System.Net.Http.HttpClientHandler();
                    handler.ServerCertificateCustomValidationCallback = (msg, cert, chain, errors) => true;
                }
            }
            """,
            "CSD0011");
    }

    [Fact]
    public async Task ShouldNotReport_WhenNoCustomCallback()
    {
        await AnalyzerTest<Csd0011HttpClientCertBypassAnalyzer>.RunAsync(
            """
            class C { void M() { var c = new System.Net.Http.HttpClient(); } }
            """);
    }
}
