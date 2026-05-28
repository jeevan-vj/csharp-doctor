using CSharpDoctor.Analyzers.Rules.AspNetCore;
using Xunit;

namespace CSharpDoctor.Analyzers.UnitTests.Rules.AspNetCore;

public sealed class Csd0411Tests
{
    [Fact]
    public async Task ShouldReport_WhenCookieSecurePolicyNone()
    {
        await AnalyzerTest<Csd0411InsecureCookiePolicyAnalyzer>.RunAsync(
            """
            class Startup {
                void Configure() {
                    var options = new CookiePolicyOptions { Secure = CookieSecurePolicy.None };
                }
            }
            class CookiePolicyOptions { public CookieSecurePolicy Secure { get; set; } }
            enum CookieSecurePolicy { None, Always }
            """,
            "CSD0411");
    }

    [Fact]
    public async Task ShouldNotReport_WhenCookieSecureAlways()
    {
        await AnalyzerTest<Csd0411InsecureCookiePolicyAnalyzer>.RunAsync(
            """
            class Startup {
                void Configure() {
                    var options = new CookiePolicyOptions { Secure = CookieSecurePolicy.Always };
                }
            }
            class CookiePolicyOptions { public CookieSecurePolicy Secure { get; set; } }
            enum CookieSecurePolicy { None, Always }
            """);
    }
}
