using CSharpDoctor.Core.Configuration;
using CSharpDoctor.Core.Diagnostics;
using CSharpDoctor.Core.Reporting;
using Shouldly;
using Xunit;

namespace CSharpDoctor.Core.UnitTests.Reporting;

public sealed class QualityGateEvaluatorTests
{
    [Fact]
    public void Evaluate_FailsMaxErrors_WhenOverLimit()
    {
        var diagnostics = new[] { Record(DiagnosticSeverity.Error) };
        var metrics = ScanMetricsCalculator.Compute(diagnostics, new DebtWeightsConfig());
        var config = new QualityGatesConfig { Enabled = true, MaxErrors = 0 };

        var evaluation = QualityGateEvaluator.Evaluate(diagnostics, metrics, config, baselineIdentities: null);

        evaluation.Passed.ShouldBeFalse();
        evaluation.Gates.ShouldContain(g => g.Name == "maxErrors" && !g.Passed && g.Actual == 1);
    }

    [Fact]
    public void Evaluate_PassesMaxDebtScore_WhenUnderLimit()
    {
        var diagnostics = new[] { Record(DiagnosticSeverity.Warning) };
        var metrics = ScanMetricsCalculator.Compute(diagnostics, new DebtWeightsConfig());
        var config = new QualityGatesConfig { Enabled = true, MaxDebtScore = 10 };

        var evaluation = QualityGateEvaluator.Evaluate(diagnostics, metrics, config, baselineIdentities: null);

        evaluation.Passed.ShouldBeTrue();
    }

    [Fact]
    public void Evaluate_FailsMaxIssuesPerRule()
    {
        var diagnostics = new[]
        {
            Record(DiagnosticSeverity.Warning, "CSD0001"),
            Record(DiagnosticSeverity.Warning, "CSD0001"),
        };
        var metrics = ScanMetricsCalculator.Compute(diagnostics, new DebtWeightsConfig());
        var config = new QualityGatesConfig
        {
            Enabled = true,
            MaxIssuesPerRule = new Dictionary<string, int> { ["csharp-doctor/CSD0001"] = 1 },
        };

        var evaluation = QualityGateEvaluator.Evaluate(diagnostics, metrics, config, baselineIdentities: null);

        evaluation.Passed.ShouldBeFalse();
        evaluation.Gates.ShouldContain(g => g.Name == "maxIssuesPerRule" && g.Target == "csharp-doctor/CSD0001");
    }

    [Fact]
    public void Evaluate_FailsMaxNewIssues_WhenBaselineProvided()
    {
        var diagnostics = new[] { Record(DiagnosticSeverity.Warning, file: "new.cs") };
        var metrics = ScanMetricsCalculator.Compute(diagnostics, new DebtWeightsConfig());
        var baseline = new HashSet<string>(StringComparer.Ordinal)
        {
            DiagnosticIdentity.Build("old.cs", 1, 1, "csharp-doctor", "CSD0001"),
        };
        var config = new QualityGatesConfig { Enabled = true, MaxNewIssues = 0 };

        var evaluation = QualityGateEvaluator.Evaluate(diagnostics, metrics, config, baseline);

        evaluation.Passed.ShouldBeFalse();
        evaluation.Gates.ShouldContain(g => g.Name == "maxNewIssues");
    }

    [Fact]
    public void Evaluate_WhenDisabled_ReturnsPassedWithNoGates()
    {
        var evaluation = QualityGateEvaluator.Evaluate(
            [],
            ScanMetricsCalculator.Compute([], new DebtWeightsConfig()),
            new QualityGatesConfig { Enabled = false },
            baselineIdentities: null);

        evaluation.Passed.ShouldBeTrue();
        evaluation.Gates.ShouldBeEmpty();
    }

    private static DiagnosticRecord Record(
        DiagnosticSeverity severity,
        string rule = "CSD0001",
        string file = "a.cs") =>
        new(file, "csharp-doctor", rule, severity, "m", "h", 1, 1, "Security", []);
}
