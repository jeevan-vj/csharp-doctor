using CSharpDoctor.Analyzers.Rules.AspNetCore;
using Xunit;

namespace CSharpDoctor.Analyzers.UnitTests.Rules.AspNetCore;

public sealed class Csd0405Tests
{
    [Fact]
    public async Task ShouldReport_WhenAddNewtonsoftJsonUsed()
    {
        await AnalyzerTest<Csd0405NewtonsoftJsonAnalyzer>.RunAsync(
            """
            class Startup {
                void Configure(object services) {
                    services.AddControllers().AddNewtonsoftJson();
                }
            }
            static class MvcExtensions {
                public static object AddControllers(this object s) => s;
                public static object AddNewtonsoftJson(this object s) => s;
            }
            """,
            "CSD0405");
    }
}
