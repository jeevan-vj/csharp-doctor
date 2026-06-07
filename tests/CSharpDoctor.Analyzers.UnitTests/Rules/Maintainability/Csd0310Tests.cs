using CSharpDoctor.Analyzers.Rules.Maintainability;
using Xunit;

namespace CSharpDoctor.Analyzers.UnitTests.Rules.Maintainability;

public sealed class Csd0310Tests
{
    [Fact]
    public async Task ShouldReport_WhenUnmaintainable()
    {
        await AnalyzerTest<Csd0310UnmaintainableCodeAnalyzer>.RunAsync(
            """
            class P {
                void M(int x) {
                    if (x == 1) x += 1;
                    if (x == 2) x += 2;
                    if (x == 3) x += 3;
                    if (x == 4) x += 4;
                    if (x == 5) x += 5;
                    if (x == 6) x += 6;
                    if (x == 7) x += 7;
                    if (x == 8) x += 8;
                    if (x == 9) x += 9;
                    if (x == 10) x += 10;
                    if (x == 11) x += 11;
                    if (x == 12) x += 12;
                    if (x == 13) x += 13;
                    if (x == 14) x += 14;
                    if (x == 15) x += 15;
                    if (x == 16) x += 16;
                    if (x == 17) x += 17;
                    if (x == 18) x += 18;
                    if (x == 19) x += 19;
                    if (x == 20) x += 20;
                    if (x == 21) x += 21;
                    if (x == 22) x += 22;
                    if (x == 23) x += 23;
                    if (x == 24) x += 24;
                    if (x == 25) x += 25;
                    for (var i = 0; i < 10; i++) { if (i % 2 == 0) x++; else x--; }
                    if (x == 101) x -= 1;
                    if (x == 102) x -= 2;
                    if (x == 103) x -= 3;
                    if (x == 104) x -= 4;
                    if (x == 105) x -= 5;
                    if (x == 106) x -= 6;
                    if (x == 107) x -= 7;
                    if (x == 108) x -= 8;
                    if (x == 109) x -= 9;
                    if (x == 110) x -= 10;
                    if (x == 111) x -= 11;
                    if (x == 112) x -= 12;
                    if (x == 113) x -= 13;
                    if (x == 114) x -= 14;
                    if (x == 115) x -= 15;
                    if (x == 116) x -= 16;
                    if (x == 117) x -= 17;
                    if (x == 118) x -= 18;
                    if (x == 119) x -= 19;
                    if (x == 120) x -= 20;
                    if (x == 121) x -= 21;
                    if (x == 122) x -= 22;
                    if (x == 123) x -= 23;
                    if (x == 124) x -= 24;
                    if (x == 125) x -= 25;
                }
            }
            """,
            "CSD0310");
    }
}
