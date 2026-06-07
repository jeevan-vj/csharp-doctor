using CSharpDoctor.Analyzers.Rules.Maintainability;
using Xunit;

namespace CSharpDoctor.Analyzers.UnitTests.Rules.Maintainability;

public sealed class Csd0309Tests
{
    [Fact]
    public async Task ShouldReport_WhenTooManyFields()
    {
        var fields = string.Join('\n', Enumerable.Range(0, 12).Select(i => $"    private int _f{i};"));
        await AnalyzerTest<Csd0309TooManyFieldsAnalyzer>.RunAsync(
            $"class Big {{\n{fields}\n}}",
            "CSD0309");
    }
}
