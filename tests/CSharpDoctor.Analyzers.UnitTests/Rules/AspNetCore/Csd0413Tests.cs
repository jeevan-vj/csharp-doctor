using CSharpDoctor.Analyzers.Rules.AspNetCore;
using Xunit;

namespace CSharpDoctor.Analyzers.UnitTests.Rules.AspNetCore;

public sealed class Csd0413Tests
{
    [Fact]
    public async Task ShouldReport_WhenAllowAnonymousOnControllerWithAuthentication()
    {
        await AnalyzerTest<Csd0413AllowAnonymousWithAuthAnalyzer>.RunAsync(
            """
            class Program {
                static void Main() {
                    var builder = WebApplication.CreateBuilder();
                    builder.Services.AddAuthentication();
                }
            }
            [AllowAnonymous]
            class PublicController { }
            class WebApplication { public static object CreateBuilder() => new object(); }
            static class AuthExtensions { public static object AddAuthentication(this object s) => s; }
            class AllowAnonymousAttribute : System.Attribute { }
            """,
            "CSD0413");
    }

    [Fact]
    public async Task ShouldNotReport_WhenNoAuthenticationConfigured()
    {
        await AnalyzerTest<Csd0413AllowAnonymousWithAuthAnalyzer>.RunAsync(
            """
            [AllowAnonymous]
            class PublicController { }
            class AllowAnonymousAttribute : System.Attribute { }
            """);
    }
}
