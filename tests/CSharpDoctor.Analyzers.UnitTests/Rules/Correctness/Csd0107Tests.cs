using CSharpDoctor.Analyzers.Rules.Correctness;
using Xunit;

namespace CSharpDoctor.Analyzers.UnitTests.Rules.Correctness;

public sealed class Csd0107Tests
{
    [Fact]
    public async Task ShouldReport_WhenDisposableFieldNotDisposedInDispose()
    {
        await AnalyzerTest<Csd0107ClassDisposeAnalyzer>.RunAsync(
            """
            using System.IO;
            class C : System.IDisposable {
                private readonly FileStream _stream = new FileStream("a.txt", FileMode.Open);
                public void Dispose() { }
            }
            """,
            "CSD0107");
    }

    [Fact]
    public async Task ShouldNotReport_WhenFieldDisposed()
    {
        await AnalyzerTest<Csd0107ClassDisposeAnalyzer>.RunAsync(
            """
            using System.IO;
            class C : System.IDisposable {
                private readonly FileStream _stream = new FileStream("a.txt", FileMode.Open);
                public void Dispose() { _stream.Dispose(); }
            }
            """);
    }
}
