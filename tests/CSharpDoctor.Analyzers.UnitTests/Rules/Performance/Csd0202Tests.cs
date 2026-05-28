using CSharpDoctor.Analyzers.Rules.Performance;
using Xunit;

namespace CSharpDoctor.Analyzers.UnitTests.Rules.Performance;

public sealed class Csd0202Tests
{
    [Fact]
    public async Task ShouldReport_WhenStringConcatInLoop() =>
        await AnalyzerTest<Csd0202StringConcatInLoopAnalyzer>.RunAsync(
            """
            class C {
                string M() {
                    var s = "";
                    for (var i = 0; i < 3; i++) { s += "x"; }
                    return s;
                }
            }
            """,
            "CSD0202");

    [Fact]
    public async Task ShouldNotReport_WhenOutsideLoop() =>
        await AnalyzerTest<Csd0202StringConcatInLoopAnalyzer>.RunAsync(
            """
            class C { string M() { var s = ""; s += "x"; return s; } }
            """);
}
