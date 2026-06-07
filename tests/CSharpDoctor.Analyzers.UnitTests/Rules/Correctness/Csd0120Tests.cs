using CSharpDoctor.Analyzers.Rules.Correctness;
using Xunit;

namespace CSharpDoctor.Analyzers.UnitTests.Rules.Correctness;

public sealed class Csd0120Tests
{
    [Fact]
    public async Task ShouldReport_WhenDllImportOutsideNativeMethods()
    {
        await AnalyzerTest<Csd0120NativeMethodsClassAnalyzer>.RunAsync(
            """
            using System.Runtime.InteropServices;
            class Interop {
                [DllImport("kernel32")] private static extern int GetTickCount();
            }
            """,
            "CSD0120");
    }

    [Fact]
    public async Task ShouldNotReport_WhenInsideNativeMethods()
    {
        await AnalyzerTest<Csd0120NativeMethodsClassAnalyzer>.RunAsync(
            """
            using System.Runtime.InteropServices;
            static class NativeMethods {
                [DllImport("kernel32")] internal static extern int GetTickCount();
            }
            """);
    }
}
