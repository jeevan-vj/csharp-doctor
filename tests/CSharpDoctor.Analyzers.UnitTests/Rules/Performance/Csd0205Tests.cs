using CSharpDoctor.Analyzers.Rules.Performance;
using Xunit;

namespace CSharpDoctor.Analyzers.UnitTests.Rules.Performance;

public sealed class Csd0205Tests
{
    [Fact]
    public async Task ShouldReport_WhenEnumerableEnumeratedTwice()
    {
        await AnalyzerTest<Csd0205MultipleEnumerationAnalyzer>.RunAsync(
            """
            using System.Collections.Generic;
            using System.Linq;
            class C {
                int M(IEnumerable<int> items) {
                    if (items.Count() == 0) return 0;
                    var sum = 0;
                    foreach (var x in items) sum += x;
                    return sum;
                }
            }
            """,
            "CSD0205");
    }

    [Fact]
    public async Task ShouldNotReport_WhenMaterializedOnce()
    {
        await AnalyzerTest<Csd0205MultipleEnumerationAnalyzer>.RunAsync(
            """
            using System.Collections.Generic;
            using System.Linq;
            class C {
                int M(IEnumerable<int> items) {
                    var list = items.ToList();
                    return list.Count;
                }
            }
            """);
    }
}
