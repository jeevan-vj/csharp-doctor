using CSharpDoctor.Analyzers.Rules.Correctness;
using Xunit;

namespace CSharpDoctor.Analyzers.UnitTests.Rules.Correctness;

public sealed class Csd0111Tests
{
    [Fact]
    public async Task ShouldReport_WhenParseWithoutCulture()
    {
        await AnalyzerTest<Csd0111CultureAwareParsingAnalyzer>.RunAsync(
            """
            class P { int M(string s) => int.Parse(s); }
            """,
            "CSD0111");
    }

    [Fact]
    public async Task ShouldNotReport_WhenParseWithInvariantCulture()
    {
        await AnalyzerTest<Csd0111CultureAwareParsingAnalyzer>.RunAsync(
            """
            using System.Globalization;
            class P { int M(string s) => int.Parse(s, CultureInfo.InvariantCulture); }
            """);
    }
}
