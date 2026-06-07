using CSharpDoctor.Analyzers.Rules.Maintainability;
using Xunit;

namespace CSharpDoctor.Analyzers.UnitTests.Rules.Maintainability;

public sealed class Csd0303Tests
{
    [Fact]
    public async Task ShouldReport_WhenMutableStaticField()
    {
        await AnalyzerTest<Csd0303NonReadonlyStaticFieldAnalyzer>.RunAsync(
            """class P { static int _count; }""",
            "CSD0303");
    }
}
