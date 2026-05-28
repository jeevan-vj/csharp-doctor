using CSharpDoctor.Analyzers.Rules.AspNetCore;
using Xunit;

namespace CSharpDoctor.Analyzers.UnitTests.Rules.AspNetCore;

public sealed class Csd0410Tests
{
    [Fact]
    public async Task ShouldReport_WhenHttpPostWithoutAntiForgery()
    {
        await AnalyzerTest<Csd0410MissingAntiForgeryAnalyzer>.RunAsync(
            """
            class OrdersController {
                [HttpPost]
                public void Create() { }
            }
            class HttpPostAttribute : System.Attribute { }
            """,
            "CSD0410");
    }

    [Fact]
    public async Task ShouldNotReport_WhenValidateAntiForgeryPresent()
    {
        await AnalyzerTest<Csd0410MissingAntiForgeryAnalyzer>.RunAsync(
            """
            class OrdersController {
                [HttpPost]
                [ValidateAntiForgeryToken]
                public void Create() { }
            }
            class HttpPostAttribute : System.Attribute { }
            class ValidateAntiForgeryTokenAttribute : System.Attribute { }
            """);
    }
}
