using CSharpDoctor.Analyzers.Rules.Correctness;
using Xunit;

namespace CSharpDoctor.Analyzers.UnitTests.Rules.Correctness;

public sealed class Csd0112Tests
{
    [Fact]
    public async Task ShouldReport_WhenConcreteMethodThrowsNotImplemented()
    {
        await AnalyzerTest<Csd0112NotImplementedExceptionAnalyzer>.RunAsync(
            """
            class P { public void M() { throw new System.NotImplementedException(); } }
            """,
            "CSD0112");
    }

    [Fact]
    public async Task ShouldNotReport_WhenAbstractMethod()
    {
        await AnalyzerTest<Csd0112NotImplementedExceptionAnalyzer>.RunAsync(
            """
            abstract class P { public abstract void M(); }
            """);
    }
}
