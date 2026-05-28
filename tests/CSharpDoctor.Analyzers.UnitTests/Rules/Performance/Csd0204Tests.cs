using CSharpDoctor.Analyzers.Rules.Performance;
using Xunit;

namespace CSharpDoctor.Analyzers.UnitTests.Rules.Performance;

public sealed class Csd0204Tests
{
    [Fact]
    public async Task ShouldReport_WhenHttpClientCreatedInLoop()
    {
        await AnalyzerTest<Csd0204HttpClientInLoopAnalyzer>.RunAsync(
            """
            class C {
                async System.Threading.Tasks.Task M(string[] urls) {
                    for (var i = 0; i < urls.Length; i++) {
                        using var client = new System.Net.Http.HttpClient();
                        await client.GetAsync(urls[i]);
                    }
                }
            }
            """,
            "CSD0204");
    }

    [Fact]
    public async Task ShouldNotReport_WhenHttpClientOutsideLoop()
    {
        await AnalyzerTest<Csd0204HttpClientInLoopAnalyzer>.RunAsync(
            """
            class C {
                async System.Threading.Tasks.Task M() {
                    using var client = new System.Net.Http.HttpClient();
                    await client.GetAsync("https://example.com");
                }
            }
            """);
    }
}
