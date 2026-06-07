using CSharpDoctor.Core.Configuration;
using CSharpDoctor.Core.Diagnostics;
using CSharpDoctor.Core.Reporting;
using Shouldly;
using Xunit;

namespace CSharpDoctor.Core.UnitTests.Reporting;

public sealed class HotSpotRankerTests
{
    [Fact]
    public void Rank_OrdersRulesByDebtScoreDescending()
    {
        var diagnostics = new[]
        {
            Record(DiagnosticSeverity.Error, "CSD0001"),
            Record(DiagnosticSeverity.Warning, "CSD0002"),
            Record(DiagnosticSeverity.Warning, "CSD0002"),
        };
        var metrics = ScanMetricsCalculator.Compute(diagnostics, new DebtWeightsConfig());

        var report = HotSpotRanker.Rank(metrics, new HotSpotsConfig { TopRules = 5, TopFiles = 5, TopElements = 5 });

        report.IssuesByRule[0].Key.ShouldBe("csharp-doctor/CSD0002");
        report.IssuesByRule[0].PriorityScore.ShouldBeGreaterThan(report.IssuesByRule[1].PriorityScore);
    }

    [Fact]
    public void Rank_LimitsTopRulesAndFiles()
    {
        var diagnostics = Enumerable.Range(0, 12)
            .Select(i => Record(DiagnosticSeverity.Warning, $"CSD{i:0000}", $"file{i}.cs"))
            .ToArray();
        var metrics = ScanMetricsCalculator.Compute(diagnostics, new DebtWeightsConfig());

        var report = HotSpotRanker.Rank(metrics, new HotSpotsConfig { TopRules = 3, TopFiles = 4, TopElements = 5 });

        report.IssuesByRule.Count.ShouldBe(3);
        report.IssuesByFile.Count.ShouldBe(4);
        report.FixPriority.Count.ShouldBe(5);
    }

    private static DiagnosticRecord Record(DiagnosticSeverity severity, string rule, string file = "a.cs") =>
        new(file, "csharp-doctor", rule, severity, "m", "h", 1, 1, "Security", []);
}
