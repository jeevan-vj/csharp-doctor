using CSharpDoctor.Analyzers.Rules.Correctness;
using Xunit;

namespace CSharpDoctor.Analyzers.UnitTests.Rules.Correctness;

public sealed class Csd0117Tests
{
    [Fact]
    public async Task ShouldReport_WhenEventSubscribedButNotUnsubscribedInDispose()
    {
        await AnalyzerTest<Csd0117DisposableEventUnsubscribeAnalyzer>.RunAsync(
            """
            using System;
            class Source { public event EventHandler? Changed; }
            class P : IDisposable {
                public P(Source s) { s.Changed += OnChanged; }
                void OnChanged(object? sender, EventArgs e) { }
                public void Dispose() { }
            }
            """,
            "CSD0117");
    }
}
