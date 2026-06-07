using CSharpDoctor.Analyzers.Rules.Correctness;
using Xunit;

namespace CSharpDoctor.Analyzers.UnitTests.Rules.Correctness;

public sealed class Csd0114Tests
{
    [Fact]
    public async Task ShouldReport_WhenInstanceMethodAssignsStaticField()
    {
        await AnalyzerTest<Csd0114StaticFieldFromInstanceAnalyzer>.RunAsync(
            """
            class P {
                static int _count;
                void M() { _count = 1; }
            }
            """,
            "CSD0114");
    }
}
