using CSharpDoctor.Analyzers.Rules.AspNetCore;
using Xunit;

namespace CSharpDoctor.Analyzers.UnitTests.Rules.AspNetCore;

public sealed class Csd0408Tests
{
    [Fact]
    public async Task ShouldReport_WhenWriteAsyncInMiddlewareWithoutTryFinally()
    {
        await AnalyzerTest<Csd0408MiddlewareWriteAsyncAnalyzer>.RunAsync(
            """
            class Program {
                static void M(object app) {
                    app.Use(async (context, next) => {
                        await context.Response.WriteAsync("ok");
                        await next();
                    });
                }
            }
            class HttpResponse { public System.Threading.Tasks.Task WriteAsync(string s) => System.Threading.Tasks.Task.CompletedTask; }
            class HttpContext { public HttpResponse Response { get; } = new HttpResponse(); }
            static class MiddlewareExtensions {
                public static object Use(this object app, object middleware) => app;
            }
            """,
            "CSD0408");
    }

    [Fact]
    public async Task ShouldNotReport_WhenWriteAsyncInsideTryFinally()
    {
        await AnalyzerTest<Csd0408MiddlewareWriteAsyncAnalyzer>.RunAsync(
            """
            class Program {
                static void M(object app) {
                    app.Use(async (context, next) => {
                        try {
                            await context.Response.WriteAsync("ok");
                        } finally {
                            await next();
                        }
                    });
                }
            }
            class HttpResponse { public System.Threading.Tasks.Task WriteAsync(string s) => System.Threading.Tasks.Task.CompletedTask; }
            class HttpContext { public HttpResponse Response { get; } = new HttpResponse(); }
            static class MiddlewareExtensions {
                public static object Use(this object app, object middleware) => app;
            }
            """);
    }
}
