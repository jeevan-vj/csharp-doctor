using CSharpDoctor.Analyzers.Rules.Correctness;
using Xunit;

namespace CSharpDoctor.Analyzers.UnitTests.Rules.Correctness;

public sealed class Csd0101Tests
{
    [Fact]
    public async Task ShouldReport_WhenTaskNotAwaited() =>
        await AnalyzerTest<Csd0101UnobservedTaskAnalyzer>.RunAsync(
            """
            using System.Threading.Tasks;
            class C { void M() { SendAsync(); } static Task SendAsync() => Task.CompletedTask; }
            """,
            "CSD0101");

    [Fact]
    public async Task ShouldNotReport_WhenTaskAwaited() =>
        await AnalyzerTest<Csd0101UnobservedTaskAnalyzer>.RunAsync(
            """
            using System.Threading.Tasks;
            class C { async Task M() { await SendAsync(); } static Task SendAsync() => Task.CompletedTask; }
            """);
}
