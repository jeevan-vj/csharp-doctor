using System.Collections.Immutable;
using CSharpDoctor.Core.Analysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Shouldly;

namespace CSharpDoctor.Analyzers.UnitTests;

internal static class AnalyzerTest<TAnalyzer>
    where TAnalyzer : DiagnosticAnalyzer, new()
{
    public static async Task RunAsync(
        string source,
        params string[] expectedRuleIds)
    {
        var path = Path.Combine(Path.GetTempPath(), $"csd-test-{Guid.NewGuid():N}.cs");
        await File.WriteAllTextAsync(path, source).ConfigureAwait(false);

        try
        {
            var analyzers = ImmutableArray.Create<DiagnosticAnalyzer>(new TAnalyzer());
            var diagnostics = await AnalyzerRunner.RunAsync([path], analyzers).ConfigureAwait(false);
            var ruleIds = diagnostics.Select(static d => d.Rule).ToArray();

            foreach (var expected in expectedRuleIds)
            {
                ruleIds.ShouldContain(expected);
            }

            if (expectedRuleIds.Length == 0)
            {
                diagnostics.ShouldBeEmpty();
            }
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}
