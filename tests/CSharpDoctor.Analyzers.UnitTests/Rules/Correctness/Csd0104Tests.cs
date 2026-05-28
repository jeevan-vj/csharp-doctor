using CSharpDoctor.Analyzers.Rules.Correctness;
using Xunit;

namespace CSharpDoctor.Analyzers.UnitTests.Rules.Correctness;

public sealed class Csd0104Tests
{
    [Fact]
    public async Task ShouldReport_WhenForgivingAppliedToNullableReturn()
    {
        await AnalyzerTest<Csd0104NullableForgivingMisuseAnalyzer>.RunAsync(
            """
            #nullable enable
            class C {
                string? GetName() => null;
                void M() {
                    var name = GetName()!;
                }
            }
            """,
            "CSD0104");
    }

    [Fact]
    public async Task ShouldNotReport_WhenNoForgivingOperator()
    {
        await AnalyzerTest<Csd0104NullableForgivingMisuseAnalyzer>.RunAsync(
            """
            #nullable enable
            class C {
                string? GetName() => null;
                void M() {
                    var name = GetName();
                }
            }
            """);
    }
}
