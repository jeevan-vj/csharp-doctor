using CSharpDoctor.Analyzers.Rules.Security;
using Xunit;

namespace CSharpDoctor.Analyzers.UnitTests.Rules.Security;

public sealed class Csd0007Tests
{
    [Fact]
    public async Task ShouldReport_WhenFromSqlRawUsesInterpolation()
    {
        await AnalyzerTest<Csd0007RawSqlInterpolationAnalyzer>.RunAsync(
            """
            class Db {
                void M(string id) {
                    var q = FromSqlRaw($"SELECT * FROM Users WHERE Id = {id}");
                }
                object FromSqlRaw(string sql) => null;
            }
            """,
            "CSD0007");
    }

    [Fact]
    public async Task ShouldNotReport_WhenFromSqlRawUsesLiteral()
    {
        await AnalyzerTest<Csd0007RawSqlInterpolationAnalyzer>.RunAsync(
            """
            class Db {
                void M() {
                    var q = FromSqlRaw("SELECT 1");
                }
                object FromSqlRaw(string sql) => null;
            }
            """);
    }
}
