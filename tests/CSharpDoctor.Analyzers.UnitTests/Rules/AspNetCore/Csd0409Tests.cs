using CSharpDoctor.Analyzers.Rules.AspNetCore;
using Xunit;

namespace CSharpDoctor.Analyzers.UnitTests.Rules.AspNetCore;

public sealed class Csd0409Tests
{
    [Fact]
    public async Task ShouldReport_WhenSwaggerEnabledWithoutDevelopmentGuard()
    {
        await AnalyzerTest<Csd0409SwaggerExposureAnalyzer>.RunAsync(
            """
            class Program {
                static void Main() {
                    var app = WebApplication.CreateBuilder().Build();
                    app.UseSwagger();
                    app.Run();
                }
            }
            class WebApplication {
                public static object CreateBuilder() => new object();
                public static void Run(this object app) { }
                public static void UseSwagger(this object app) { }
                public static object Build(this object builder) => builder;
            }
            """,
            "CSD0409");
    }

    [Fact]
    public async Task ShouldNotReport_WhenSwaggerOnlyInDevelopment()
    {
        await AnalyzerTest<Csd0409SwaggerExposureAnalyzer>.RunAsync(
            """
            class Program {
                static void Main() {
                    var app = WebApplication.CreateBuilder().Build();
                    if (app.Environment.IsDevelopment()) app.UseSwagger();
                    app.Run();
                }
            }
            class WebApplication {
                public static object CreateBuilder() => new object();
                public static void Run(this object app) { }
                public static void UseSwagger(this object app) { }
                public static object Build(this object builder) => builder;
                public object Environment { get; } = new Env();
            }
            class Env { public bool IsDevelopment() => true; }
            """);
    }
}
