using CSharpDoctor.Analyzers.Rules.Security;
using Xunit;

namespace CSharpDoctor.Analyzers.UnitTests.Rules.Security;

public sealed class Csd0002Tests
{
    [Fact]
    public async Task ShouldReport_WhenMd5Created() =>
        await AnalyzerTest<Csd0002WeakCryptoAnalyzer>.RunAsync(
            """
            using System.Security.Cryptography;
            class C { void M() { var h = MD5.Create(); } }
            """,
            "CSD0002");

    [Fact]
    public async Task ShouldNotReport_WhenSha256Used() =>
        await AnalyzerTest<Csd0002WeakCryptoAnalyzer>.RunAsync(
            """
            using System.Security.Cryptography;
            class C { void M() { var h = SHA256.Create(); } }
            """);
}
