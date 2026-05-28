using CSharpDoctor.Analyzers.Rules.AspNetCore;
using Xunit;

namespace CSharpDoctor.Analyzers.UnitTests.Rules.AspNetCore;

public sealed class Csd0414Tests
{
    [Fact]
    public async Task ShouldReport_WhenAddAuthenticationWithoutUseAuthentication()
    {
        await AnalyzerTest<Csd0414AuthMiddlewareMissingAnalyzer>.RunAsync(
            """
            class Program {
                static void Main() {
                    var builder = WebApplication.CreateBuilder();
                    builder.Services.AddAuthentication();
                    var app = builder.Build();
                    app.Run();
                }
            }
            class WebApplication {
                public static object CreateBuilder() => new object();
                public static object Build(this object b) => b;
                public static void Run(this object app) { }
            }
            static class AuthExtensions { public static object AddAuthentication(this object s) => s; }
            """,
            "CSD0414");
    }

    [Fact]
    public async Task ShouldNotReport_WhenUseAuthenticationPresent()
    {
        await AnalyzerTest<Csd0414AuthMiddlewareMissingAnalyzer>.RunAsync(
            """
            class Program {
                static void Main() {
                    var builder = WebApplication.CreateBuilder();
                    builder.Services.AddAuthentication();
                    var app = builder.Build();
                    app.UseAuthentication();
                    app.Run();
                }
            }
            class WebApplication {
                public static object CreateBuilder() => new object();
                public static object Build(this object b) => b;
                public static void Run(this object app) { }
                public static void UseAuthentication(this object app) { }
            }
            static class AuthExtensions { public static object AddAuthentication(this object s) => s; }
            """);
    }
}
