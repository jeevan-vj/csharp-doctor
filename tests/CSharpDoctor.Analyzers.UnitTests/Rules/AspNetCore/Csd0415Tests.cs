using CSharpDoctor.Analyzers.Rules.AspNetCore;
using Xunit;

namespace CSharpDoctor.Analyzers.UnitTests.Rules.AspNetCore;

public sealed class Csd0415Tests
{
    [Fact]
    public async Task ShouldReport_WhenEnableSensitiveDataLogging()
    {
        await AnalyzerTest<Csd0415EfSensitiveLoggingAnalyzer>.RunAsync(
            """
            class Startup {
                void Configure(object services) {
                    services.AddDbContext<AppDbContext>(o => o.EnableSensitiveDataLogging());
                }
            }
            class AppDbContext { }
            static class EfExtensions {
                public static object AddDbContext<T>(this object s, System.Action<object> configure) => s;
            }
            static class DbContextOptionsBuilderExtensions {
                public static object EnableSensitiveDataLogging(this object o) => o;
            }
            """,
            "CSD0415");
    }

    [Fact]
    public async Task ShouldNotReport_WhenSensitiveLoggingNotEnabled()
    {
        await AnalyzerTest<Csd0415EfSensitiveLoggingAnalyzer>.RunAsync(
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
