using CSharpDoctor.Analyzers.Rules.Correctness;
using Xunit;

namespace CSharpDoctor.Analyzers.UnitTests.Rules.Correctness;

public sealed class Csd0103Tests
{
    [Fact]
    public async Task ShouldReport_WhenResultUsedInAsyncMethod() =>
        await AnalyzerTest<Csd0103SyncOverAsyncAnalyzer>.RunAsync(
            """
            using System.Threading.Tasks;
            class C {
                async Task M(Task<int> t) { var x = t.Result; }
            }
            """,
            "CSD0103");

    [Fact]
    public async Task ShouldNotReport_WhenAwaited() =>
        await AnalyzerTest<Csd0103SyncOverAsyncAnalyzer>.RunAsync(
            """
            using System.Threading.Tasks;
            class C {
                async Task M(Task<int> t) { var x = await t; }
            }
            """);
}
