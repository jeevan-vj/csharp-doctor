using CSharpDoctor.Analyzers.Rules.Correctness;
using Xunit;

namespace CSharpDoctor.Analyzers.UnitTests.Rules.Correctness;

public sealed class Csd0115Tests
{
    [Fact]
    public async Task ShouldReport_WhenMonitorEnterWithoutExit()
    {
        await AnalyzerTest<Csd0115MonitorLockPairingAnalyzer>.RunAsync(
            """
            using System.Threading;
            class P {
                void M(object o) { Monitor.Enter(o); }
            }
            """,
            "CSD0115");
    }
}
