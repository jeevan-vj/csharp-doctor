using CSharpDoctor.Analyzers.Rules.Maintainability;
using Xunit;

namespace CSharpDoctor.Analyzers.UnitTests.Rules.Maintainability;

public sealed class Csd0305Tests
{
    [Fact]
    public async Task ShouldReport_WhenMethodTooComplex()
    {
        await AnalyzerTest<Csd0305MethodTooComplexAnalyzer>.RunAsync(
            """
            class P {
                int M(int x) {
                    if (x == 1) return 1;
                    if (x == 2) return 2;
                    if (x == 3) return 3;
                    if (x == 4) return 4;
                    if (x == 5) return 5;
                    if (x == 6) return 6;
                    if (x == 7) return 7;
                    if (x == 8) return 8;
                    if (x == 9) return 9;
                    if (x == 10) return 10;
                    if (x == 11) return 11;
                    if (x == 12) return 12;
                    if (x == 13) return 13;
                    return 0;
                }
            }
            """,
            "CSD0305");
    }
}
