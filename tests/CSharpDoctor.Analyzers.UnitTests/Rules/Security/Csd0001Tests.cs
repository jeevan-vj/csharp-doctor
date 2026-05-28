using CSharpDoctor.Analyzers.Rules.Security;
using Xunit;

namespace CSharpDoctor.Analyzers.UnitTests.Rules.Security;

public sealed class Csd0001Tests
{
    [Fact]
    public async Task ShouldReport_WhenCommandTextUsesConcatenation()
    {
        var source = """
            class SqlCommand { public string CommandText { get; set; } }
            class C {
                void M(string input) {
                    var cmd = new SqlCommand();
                    cmd.CommandText = "SELECT * FROM t WHERE id = " + input;
                }
            }
            """;

        await AnalyzerTest<Csd0001ConcatenatedSqlAnalyzer>.RunAsync(source, "CSD0001");
    }

    [Fact]
    public async Task ShouldNotReport_WhenCommandTextIsLiteral()
    {
        var source = """
            class SqlCommand { public string CommandText { get; set; } }
            class C {
                void M() {
                    var cmd = new SqlCommand();
                    cmd.CommandText = "SELECT 1";
                }
            }
            """;

        await AnalyzerTest<Csd0001ConcatenatedSqlAnalyzer>.RunAsync(source);
    }
}
