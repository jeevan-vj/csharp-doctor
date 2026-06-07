using CSharpDoctor.Analyzers.Rules.Maintainability;
using Xunit;

namespace CSharpDoctor.Analyzers.UnitTests.Rules.Maintainability;

public sealed class Csd0308Tests
{
    [Fact]
    public async Task ShouldReport_WhenTooManyMethods()
    {
        var methods = string.Join('\n', Enumerable.Range(0, 20).Select(i => $"    public int M{i}() => {i};"));
        await AnalyzerTest<Csd0308TooManyMethodsAnalyzer>.RunAsync(
            $"class Big {{\n{methods}\n}}",
            "CSD0308");
    }
}
