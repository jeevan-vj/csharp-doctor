using CSharpDoctor.Analyzers.Rules.Security;
using Xunit;

namespace CSharpDoctor.Analyzers.UnitTests.Rules.Security;

public sealed class Csd0006Tests
{
    [Fact]
    public async Task ShouldReport_WhenCatchBlockIsEmpty()
    {
        await AnalyzerTest<Csd0006EmptyCatchAnalyzer>.RunAsync(
            """
            class C {
                void M() {
                    try { Work(); }
                    catch { }
                }
                static void Work() { }
            }
            """,
            "CSD0006");
    }

    [Fact]
    public async Task ShouldNotReport_WhenCatchHandlesException()
    {
        await AnalyzerTest<Csd0006EmptyCatchAnalyzer>.RunAsync(
            """
            class C {
                void M() {
                    try { Work(); }
                    catch (System.Exception ex) { System.Console.WriteLine(ex.Message); }
                }
                static void Work() { }
            }
            """);
    }
}
