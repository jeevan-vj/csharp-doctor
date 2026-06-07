using CSharpDoctor.Analyzers.Rules.Correctness;
using Xunit;

namespace CSharpDoctor.Analyzers.UnitTests.Rules.Correctness;

public sealed class Csd0110Tests
{
    [Fact]
    public async Task ShouldReport_WhenCollectWithoutWait()
    {
        await AnalyzerTest<Csd0110GcCollectWithoutWaitAnalyzer>.RunAsync(
            """
            class P { void M() { System.GC.Collect(); } }
            """,
            "CSD0110");
    }

    [Fact]
    public async Task ShouldNotReport_WhenCollectWithWait()
    {
        await AnalyzerTest<Csd0110GcCollectWithoutWaitAnalyzer>.RunAsync(
            """
            class P {
                void M() {
                    System.GC.Collect();
                    System.GC.WaitForPendingFinalizers();
                }
            }
            """);
    }
}
