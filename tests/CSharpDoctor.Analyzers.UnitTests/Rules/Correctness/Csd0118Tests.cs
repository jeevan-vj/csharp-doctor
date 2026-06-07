using CSharpDoctor.Analyzers.Rules.Correctness;
using Xunit;

namespace CSharpDoctor.Analyzers.UnitTests.Rules.Correctness;

public sealed class Csd0118Tests
{
    [Fact]
    public async Task ShouldReport_WhenReaderWriterLockNotReleased()
    {
        await AnalyzerTest<Csd0118ReaderWriterLockPairingAnalyzer>.RunAsync(
            """
            class P {
                void M(System.Threading.ReaderWriterLock l) { l.AcquireReaderLock(1000); }
            }
            """,
            "CSD0118");
    }
}
