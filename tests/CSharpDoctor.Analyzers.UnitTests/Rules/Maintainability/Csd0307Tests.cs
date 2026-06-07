using CSharpDoctor.Analyzers.Rules.Maintainability;
using Xunit;

namespace CSharpDoctor.Analyzers.UnitTests.Rules.Maintainability;

public sealed class Csd0307Tests
{
    [Fact]
    public async Task ShouldReport_WhenTypeTooBig()
    {
        var lines = string.Join('\n', Enumerable.Range(0, 130).Select(i => $"    public int F{i}() => {i};"));
        await AnalyzerTest<Csd0307TypeTooBigAnalyzer>.RunAsync(
            $"class Big {{\n{lines}\n}}",
            "CSD0307");
    }
}
