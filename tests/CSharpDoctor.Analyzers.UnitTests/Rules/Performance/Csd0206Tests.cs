using CSharpDoctor.Analyzers.Rules.Performance;
using Xunit;

namespace CSharpDoctor.Analyzers.UnitTests.Rules.Performance;

public sealed class Csd0206Tests
{
    [Fact]
    public async Task ShouldReport_WhenStringFormatInLoop()
    {
        await AnalyzerTest<Csd0206StringFormatInLoopAnalyzer>.RunAsync(
            """
            class C {
                string M(int count) {
                    var result = "";
                    for (var i = 0; i < count; i++) {
                        result = string.Format("item {0}", i);
                    }
                    return result;
                }
            }
            """,
            "CSD0206");
    }

    [Fact]
    public async Task ShouldNotReport_WhenFormatOutsideLoop()
    {
        await AnalyzerTest<Csd0206StringFormatInLoopAnalyzer>.RunAsync(
            """
            class C { string M(int i) => string.Format("item {0}", i); }
            """);
    }
}
