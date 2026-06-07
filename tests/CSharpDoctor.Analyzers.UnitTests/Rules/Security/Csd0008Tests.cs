using CSharpDoctor.Analyzers.Rules.Security;
using Xunit;

namespace CSharpDoctor.Analyzers.UnitTests.Rules.Security;

public sealed class Csd0008Tests
{
    [Fact]
    public async Task ShouldReport_WhenPublicPointerField()
    {
        await AnalyzerTest<Csd0008PublicPointerFieldAnalyzer>.RunAsync(
            """
            unsafe class P { public int* Ptr; }
            """,
            "CSD0008");
    }
}
