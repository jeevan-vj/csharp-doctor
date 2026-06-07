using CSharpDoctor.Analyzers.Rules.Correctness;
using Xunit;

namespace CSharpDoctor.Analyzers.UnitTests.Rules.Correctness;

public sealed class Csd0116Tests
{
    [Fact]
    public async Task ShouldReport_WhenPublicDllImport()
    {
        await AnalyzerTest<Csd0116PInvokeVisibilityAnalyzer>.RunAsync(
            """
            using System.Runtime.InteropServices;
            class P {
                [DllImport("kernel32")] public static extern int GetTickCount();
            }
            """,
            "CSD0116");
    }
}
