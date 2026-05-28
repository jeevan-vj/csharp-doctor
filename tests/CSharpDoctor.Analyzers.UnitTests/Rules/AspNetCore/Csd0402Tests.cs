using CSharpDoctor.Analyzers.Rules.AspNetCore;
using Xunit;

namespace CSharpDoctor.Analyzers.UnitTests.Rules.AspNetCore;

public sealed class Csd0402Tests
{
    [Fact]
    public async Task ShouldReport_WhenDbContextRegisteredAsSingleton()
    {
        await AnalyzerTest<Csd0402DbContextSingletonAnalyzer>.RunAsync(
            """
            class AppDbContext { }
            class Startup {
                void Configure(object services) {
                    services.AddSingleton<AppDbContext>();
                }
            }
            static class ServiceExtensions {
                public static object AddSingleton<T>(this object s) => s;
            }
            """,
            "CSD0402");
    }

    [Fact]
    public async Task ShouldNotReport_WhenDbContextRegisteredScoped()
    {
        await AnalyzerTest<Csd0402DbContextSingletonAnalyzer>.RunAsync(
            """
            class AppDbContext { }
            class Startup {
                void Configure(object services) {
                    services.AddScoped<AppDbContext>();
                }
            }
            static class ServiceExtensions {
                public static object AddScoped<T>(this object s) => s;
            }
            """);
    }
}
