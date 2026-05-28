using CSharpDoctor.Analyzers.Rules.AspNetCore;
using Xunit;

namespace CSharpDoctor.Analyzers.UnitTests.Rules.AspNetCore;

public sealed class Csd0406Tests
{
    [Fact]
    public async Task ShouldReport_WhenHttpsRedirectionMissing()
    {
        await AnalyzerTest<Csd0406HttpsRedirectionAnalyzer>.RunAsync(
            """
            class Program {
                static void Main() {
                    var builder = WebApplication.CreateBuilder();
                    var app = builder.Build();
                    app.Run();
                }
            }
            class WebApplication {
                public static object CreateBuilder() => new object();
                public static void Run(this object app) { }
                public static object Build(this object builder) => builder;
            }
            """,
            "CSD0406");
    }

    [Fact]
    public async Task ShouldNotReport_WhenUseHttpsRedirectionPresent()
    {
        await AnalyzerTest<Csd0406HttpsRedirectionAnalyzer>.RunAsync(
            """
            class Program {
                static void Main() {
                    var builder = WebApplication.CreateBuilder();
                    var app = builder.Build();
                    app.UseHttpsRedirection();
                    app.Run();
                }
            }
            class WebApplication {
                public static object CreateBuilder() => new object();
                public static void Run(this object app) { }
                public static object Build(this object builder) => builder;
                public static void UseHttpsRedirection(this object app) { }
            }
            """);
    }
}
