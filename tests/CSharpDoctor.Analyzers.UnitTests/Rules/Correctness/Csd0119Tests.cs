using CSharpDoctor.Analyzers.Rules.Correctness;
using Xunit;

namespace CSharpDoctor.Analyzers.UnitTests.Rules.Correctness;

public sealed class Csd0119Tests
{
    [Fact]
    public async Task ShouldReport_WhenInstanceMethodReadsMutableFieldWithoutLock()
    {
        await AnalyzerTest<Csd0119UnsynchronizedMutableReadAnalyzer>.RunAsync(
            """
            class Counter {
                int _value;
                public int Read() => _value;
            }
            """,
            "CSD0119");
    }

    [Fact]
    public async Task ShouldNotReport_WhenMethodUsesLock()
    {
        await AnalyzerTest<Csd0119UnsynchronizedMutableReadAnalyzer>.RunAsync(
            """
            class Counter {
                int _value;
                private readonly object _gate = new();
                public int Read() { lock (_gate) { return _value; } }
            }
            """);
    }
}
