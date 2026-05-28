using CSharpDoctor.Analyzers.Rules.Correctness;
using Xunit;

namespace CSharpDoctor.Analyzers.UnitTests.Rules.Correctness;

public sealed class Csd0108Tests
{
    [Fact]
    public async Task ShouldReport_WhenLockingOnThis()
    {
        await AnalyzerTest<Csd0108LockThisAnalyzer>.RunAsync(
            """
            class C {
                void M() { lock (this) { } }
            }
            """,
            "CSD0108");
    }

    [Fact]
    public async Task ShouldNotReport_WhenLockingOnPrivateObject()
    {
        await AnalyzerTest<Csd0108LockThisAnalyzer>.RunAsync(
            """
            class C {
                private readonly object _gate = new object();
                void M() { lock (_gate) { } }
            }
            """);
    }
}
