using CSharpDoctor.Analyzers.Rules.Performance;
using Xunit;

namespace CSharpDoctor.Analyzers.UnitTests.Rules.Performance;

public sealed class Csd0201Tests
{
    [Fact]
    public async Task ShouldReport_WhenToListInsideForLoop() =>
        await AnalyzerTest<Csd0201LinqMaterializationInLoopAnalyzer>.RunAsync(
            """
            using System.Linq;
            using System.Collections.Generic;
            class C {
                void M(IEnumerable<int> src) {
                    for (var i = 0; i < 3; i++) { var list = src.ToList(); }
                }
            }
            """,
            "CSD0201");

    [Fact]
    public async Task ShouldNotReport_WhenToListOutsideLoop() =>
        await AnalyzerTest<Csd0201LinqMaterializationInLoopAnalyzer>.RunAsync(
            """
            using System.Linq;
            using System.Collections.Generic;
            class C { void M(IEnumerable<int> src) { var list = src.ToList(); } }
            """);
}
