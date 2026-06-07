using CSharpDoctor.Core.Configuration;
using CSharpDoctor.Core.Diagnostics;

namespace CSharpDoctor.Core.Reporting;

public static class QualityGateEvaluator
{
    public static QualityGateEvaluation Evaluate(
        IReadOnlyList<DiagnosticRecord> diagnostics,
        ScanMetrics metrics,
        QualityGatesConfig config,
        IReadOnlySet<string>? baselineIdentities)
    {
        if (!config.Enabled)
        {
            return new QualityGateEvaluation(Enabled: false, Passed: true, Gates: []);
        }

        var gates = new List<QualityGateResult>();
        AddGate(gates, "maxErrors", config.MaxErrors, metrics.Errors);
        AddGate(gates, "maxWarnings", config.MaxWarnings, metrics.Warnings);
        AddGate(gates, "maxTotalIssues", config.MaxTotalIssues, metrics.Total);
        AddGate(gates, "maxDebtScore", config.MaxDebtScore, metrics.DebtScore);

        var criticalIssues = diagnostics.Count(static d =>
            d.Severity == DiagnosticSeverity.Error
            && d.Category.Equals("Security", StringComparison.OrdinalIgnoreCase));
        AddGate(gates, "maxCriticalIssues", config.MaxCriticalIssues, criticalIssues);

        if (config.MaxNewIssues is not null)
        {
            var newIssues = baselineIdentities is null
                ? metrics.Total
                : diagnostics.Count(d => !baselineIdentities.Contains(DiagnosticIdentity.Build(d)));
            AddGate(gates, "maxNewIssues", config.MaxNewIssues, newIssues);
        }

        foreach (var (rule, limit) in config.MaxIssuesPerRule)
        {
            var actual = metrics.IssuesByRule.TryGetValue(rule, out var bucket) ? bucket.Count : 0;
            gates.Add(new QualityGateResult("maxIssuesPerRule", limit, actual, actual <= limit, rule));
        }

        var passed = gates.All(static g => g.Passed);
        return new QualityGateEvaluation(Enabled: true, passed, gates);
    }

    private static void AddGate(List<QualityGateResult> gates, string name, int? limit, int actual)
    {
        if (limit is null)
        {
            return;
        }

        gates.Add(new QualityGateResult(name, limit, actual, actual <= limit.Value));
    }
}
