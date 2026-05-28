using CSharpDoctor.Analyzers.Rules.AspNetCore;
using Xunit;

namespace CSharpDoctor.Analyzers.UnitTests.Rules.AspNetCore;

public sealed class Csd0407Tests
{
    [Fact]
    public async Task ShouldReport_WhenGetHandlerQueriesWithoutNoTracking()
    {
        await AnalyzerTest<Csd0407EfNoTrackingAnalyzer>.RunAsync(
            """
            class AppDbContext {
                public object Customers => null;
            }
            class CustomersController {
                [HttpGet]
                public object GetAll(AppDbContext db) {
                    return db.Customers;
                }
            }
            class HttpGetAttribute : System.Attribute { }
            """,
            "CSD0407");
    }

    [Fact]
    public async Task ShouldNotReport_WhenAsNoTrackingUsed()
    {
        await AnalyzerTest<Csd0407EfNoTrackingAnalyzer>.RunAsync(
            """
            class AppDbContext {
                public object Customers => null;
            }
            class CustomersController {
                [HttpGet]
                public object GetAll(AppDbContext db) {
                    return db.Customers.AsNoTracking();
                }
            }
            class HttpGetAttribute : System.Attribute { }
            static class EfExtensions {
                public static object AsNoTracking(this object q) => q;
            }
            """);
    }
}
