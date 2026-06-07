using CSharpDoctor.Core.Configuration;
using CSharpDoctor.Core.Diagnostics;
using CSharpDoctor.Core.Reporting;
using Shouldly;
using Xunit;

namespace CSharpDoctor.Core.UnitTests.Reporting;

public sealed class ScanMetricsCalculatorTests
{
    [Fact]
    public void Compute_CountsSeveritiesAndDebtScore()
    {
        var diagnostics = new[]
        {
            Record(DiagnosticSeverity.Error),
            Record(DiagnosticSeverity.Warning),
            Record(DiagnosticSeverity.Info),
        };

        var metrics = ScanMetricsCalculator.Compute(diagnostics, new DebtWeightsConfig());

        metrics.Errors.ShouldBe(1);
        metrics.Warnings.ShouldBe(1);
        metrics.Info.ShouldBe(1);
        metrics.Total.ShouldBe(3);
        metrics.DebtScore.ShouldBe(14);
    }

    [Fact]
    public void Compute_GroupsByRuleAndFile()
    {
        var diagnostics = new[]
        {
            Record(DiagnosticSeverity.Error, rule: "CSD0001", file: "a.cs"),
            Record(DiagnosticSeverity.Error, rule: "CSD0001", file: "b.cs"),
            Record(DiagnosticSeverity.Warning, rule: "CSD0002", file: "a.cs"),
        };

        var metrics = ScanMetricsCalculator.Compute(diagnostics, new DebtWeightsConfig());

        metrics.IssuesByRule["csharp-doctor/CSD0001"].Count.ShouldBe(2);
        metrics.IssuesByRule["csharp-doctor/CSD0001"].DebtScore.ShouldBe(20);
        metrics.IssuesByFile["a.cs"].Count.ShouldBe(2);
    }

    private static DiagnosticRecord Record(
        DiagnosticSeverity severity,
        string rule = "CSD0001",
        string file = "a.cs") =>
        new(file, "csharp-doctor", rule, severity, "m", "h", 1, 1, "Security", []);
}
