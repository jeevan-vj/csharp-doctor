using CSharpDoctor.Analyzers.Rules.AspNetCore;
using Xunit;

namespace CSharpDoctor.Analyzers.UnitTests.Rules.AspNetCore;

public sealed class Csd0401Tests
{
    [Fact]
    public async Task ShouldReport_WhenMapGetHasNoAuthorization()
    {
        await AnalyzerTest<Csd0401EndpointAuthAnalyzer>.RunAsync(
            """
            class Program {
                static void M(object app) {
                    app.MapGet("/api/data", () => "ok");
                }
            }
            class MapExtensions {
                public static object MapGet(this object app, string route, object handler) => app;
            }
            """,
            "CSD0401");
    }

    [Fact]
    public async Task ShouldNotReport_WhenRequireAuthorizationPresent()
    {
        await AnalyzerTest<Csd0401EndpointAuthAnalyzer>.RunAsync(
            """
            class Program {
                static void M(object app) {
                    app.MapGet("/api/data", () => "ok").RequireAuthorization();
                }
            }
            class MapExtensions {
                public static object MapGet(this object app, string route, object handler) => app;
                public static object RequireAuthorization(this object endpoint) => endpoint;
            }
            """);
    }
}
