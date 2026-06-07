using CSharpDoctor.Analyzers.Rules.Correctness;
using Xunit;

namespace CSharpDoctor.Analyzers.UnitTests.Rules.Correctness;

public sealed class Csd0113Tests
{
    [Fact]
    public async Task ShouldReport_WhenArrayListUsed()
    {
        await AnalyzerTest<Csd0113LegacyCollectionsAnalyzer>.RunAsync(
            """
            using System.Collections;
            class P { ArrayList M() => new ArrayList(); }
            """,
            "CSD0113");
    }
}
