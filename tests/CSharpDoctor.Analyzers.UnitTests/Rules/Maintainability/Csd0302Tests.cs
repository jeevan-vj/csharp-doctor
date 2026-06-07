using CSharpDoctor.Analyzers.Rules.Maintainability;
using Xunit;

namespace CSharpDoctor.Analyzers.UnitTests.Rules.Maintainability;

public sealed class Csd0302Tests
{
    [Fact]
    public async Task ShouldReport_WhenPublicConst()
    {
        await AnalyzerTest<Csd0302PublicConstFieldAnalyzer>.RunAsync(
            """public class P { public const int Max = 10; }""",
            "CSD0302");
    }
}
