using CSharpDoctor.Analyzers.Rules.Maintainability;
using Xunit;

namespace CSharpDoctor.Analyzers.UnitTests.Rules.Maintainability;

public sealed class Csd0306Tests
{
    [Fact]
    public async Task ShouldReport_WhenTooManyLocals()
    {
        await AnalyzerTest<Csd0306TooManyLocalVariablesAnalyzer>.RunAsync(
            """
            class P {
                void M() {
                    var a=1;var b=2;var c=3;var d=4;var e=5;var f=6;var g=7;var h=8;var i=9;
                }
            }
            """,
            "CSD0306");
    }
}
