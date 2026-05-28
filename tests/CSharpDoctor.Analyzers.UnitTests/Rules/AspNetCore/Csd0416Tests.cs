using CSharpDoctor.Analyzers.Rules.AspNetCore;
using Xunit;

namespace CSharpDoctor.Analyzers.UnitTests.Rules.AspNetCore;

public sealed class Csd0416Tests
{
    [Fact]
    public async Task ShouldReport_WhenEnableDetailedErrors()
    {
        await AnalyzerTest<Csd0416EfDetailedErrorsAnalyzer>.RunAsync(
            """
            class Startup {
                void Configure(object services) {
                    services.AddDbContext<AppDbContext>(o => o.EnableDetailedErrors());
                }
            }
            class AppDbContext { }
            static class EfExtensions {
                public static object AddDbContext<T>(this object s, System.Action<object> configure) => s;
            }
            static class DbContextOptionsBuilderExtensions {
                public static object EnableDetailedErrors(this object o) => o;
            }
            """,
            "CSD0416");
    }

    [Fact]
    public async Task ShouldNotReport_WhenDetailedErrorsNotEnabled()
    {
        await AnalyzerTest<Csd0416EfDetailedErrorsAnalyzer>.RunAsync(
            """
            class Startup {
                void Configure(object services) {
                    services.AddDbContext<AppDbContext>();
                }
            }
            class AppDbContext { }
            static class EfExtensions {
                public static object AddDbContext<T>(this object s) => s;
            }
            """);
    }
}
