using CSharpDoctor.Analyzers.Rules.AspNetCore;
using Xunit;

namespace CSharpDoctor.Analyzers.UnitTests.Rules.AspNetCore;

public sealed class Csd0403Tests
{
    [Fact]
    public async Task ShouldReport_WhenAllowAnyOriginWithAllowCredentials()
    {
        await AnalyzerTest<Csd0403CorsMisconfigurationAnalyzer>.RunAsync(
            """
            class Startup {
                void Configure() {
                    var policy = new object();
                    policy.AllowAnyOrigin().AllowCredentials();
                }
            }
            static class CorsExtensions {
                public static object AllowAnyOrigin(this object p) => p;
                public static object AllowCredentials(this object p) => p;
            }
            """,
            "CSD0403");
    }
}
