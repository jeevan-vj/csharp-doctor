using CSharpDoctor.Analyzers.Rules.Security;
using Xunit;

namespace CSharpDoctor.Analyzers.UnitTests.Rules.Security;

public sealed class Csd0003Tests
{
    [Fact]
    public async Task ShouldReport_WhenBinaryFormatterUsed() =>
        await AnalyzerTest<Csd0003InsecureDeserializationAnalyzer>.RunAsync(
            """
            using System.Runtime.Serialization.Formatters.Binary;
            class C { void M() { var f = new BinaryFormatter(); } }
            """,
            "CSD0003");

    [Fact]
    public async Task ShouldNotReport_WhenSafeCode() =>
        await AnalyzerTest<Csd0003InsecureDeserializationAnalyzer>.RunAsync("class C { int M() => 1; }");
}
