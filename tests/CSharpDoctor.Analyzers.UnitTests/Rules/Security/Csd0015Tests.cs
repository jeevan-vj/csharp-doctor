using CSharpDoctor.Analyzers.Rules.Security;
using Xunit;

namespace CSharpDoctor.Analyzers.UnitTests.Rules.Security;

public sealed class Csd0015Tests
{
    [Fact]
    public async Task ShouldReport_WhenRegexWithoutTimeout()
    {
        await AnalyzerTest<Csd0015RegexWithoutTimeoutAnalyzer>.RunAsync(
            """
            class C {
                System.Text.RegularExpressions.Regex M(string pattern) =>
                    new System.Text.RegularExpressions.Regex(pattern);
            }
            """,
            "CSD0015");
    }

    [Fact]
    public async Task ShouldNotReport_WhenRegexHasTimeout()
    {
        await AnalyzerTest<Csd0015RegexWithoutTimeoutAnalyzer>.RunAsync(
            """
            class C {
                System.Text.RegularExpressions.Regex M(string pattern) =>
                    new System.Text.RegularExpressions.Regex(pattern, System.Text.RegularExpressions.RegexOptions.None, System.TimeSpan.FromSeconds(1));
            }
            """);
    }
}
