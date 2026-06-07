using CSharpDoctor.Analyzers.Rules.Correctness;
using Xunit;

namespace CSharpDoctor.Analyzers.UnitTests.Rules.Correctness;

public sealed class Csd0109Tests
{
    [Fact]
    public async Task ShouldReport_WhenGcCollectCalled()
    {
        await AnalyzerTest<Csd0109GcCollectAnalyzer>.RunAsync(
            """
            class P { void M() { System.GC.Collect(); } }
            """,
            "CSD0109");
    }

    [Fact]
    public async Task ShouldNotReport_WhenNoGcCollect()
    {
        await AnalyzerTest<Csd0109GcCollectAnalyzer>.RunAsync(
            """class P { void M() { } }""");
    }
}
