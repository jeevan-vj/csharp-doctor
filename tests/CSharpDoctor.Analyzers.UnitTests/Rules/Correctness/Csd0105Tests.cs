using CSharpDoctor.Analyzers.Rules.Correctness;
using Xunit;

namespace CSharpDoctor.Analyzers.UnitTests.Rules.Correctness;

public sealed class Csd0105Tests
{
    [Fact]
    public async Task ShouldReport_WhenAsyncVoidMethod()
    {
        await AnalyzerTest<Csd0105AsyncVoidAnalyzer>.RunAsync(
            """
            class C {
                public async void M() { await System.Threading.Tasks.Task.CompletedTask; }
            }
            """,
            "CSD0105");
    }

    [Fact]
    public async Task ShouldNotReport_WhenAsyncTaskMethod()
    {
        await AnalyzerTest<Csd0105AsyncVoidAnalyzer>.RunAsync(
            """
            class C {
                public async System.Threading.Tasks.Task M() { await System.Threading.Tasks.Task.CompletedTask; }
            }
            """);
    }
}
