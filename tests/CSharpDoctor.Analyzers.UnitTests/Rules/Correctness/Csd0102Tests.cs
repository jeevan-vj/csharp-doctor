using CSharpDoctor.Analyzers.Rules.Correctness;
using Xunit;

namespace CSharpDoctor.Analyzers.UnitTests.Rules.Correctness;

public sealed class Csd0102Tests
{
    [Fact]
    public async Task ShouldReport_WhenDisposableNotInUsing()
    {
        await AnalyzerTest<Csd0102DisposableNotUsedAnalyzer>.RunAsync(
            """
            using System.IO;
            class C {
                void M() {
                    var stream = new FileStream("a.txt", FileMode.Open);
                }
            }
            """,
            "CSD0102");
    }

    [Fact]
    public async Task ShouldNotReport_WhenWrappedInUsing()
    {
        await AnalyzerTest<Csd0102DisposableNotUsedAnalyzer>.RunAsync(
            """
            using System.IO;
            class C {
                void M() {
                    using var stream = new FileStream("a.txt", FileMode.Open);
                }
            }
            """);
    }
}
