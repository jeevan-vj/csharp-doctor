using CSharpDoctor.Core.Diagnostics;

namespace CSharpDoctor.Core.Reporting;

public sealed record HtmlReportChecksRan(
    bool Lint,
    bool Wiring,
    bool Ef,
    bool Architecture,
    bool Sca);

public sealed record HtmlReportConfigHints(
    bool QualityGatesDisabled,
    bool NoBaseline,
    bool NoArchitecturePolicy);

public sealed record HtmlRecommendationGroups(
    IReadOnlyList<HtmlRecommendationItem> FixNow,
    IReadOnlyList<HtmlRecommendationItem> NextSprint,
    IReadOnlyList<HtmlRecommendationItem> Watch,
    IReadOnlyList<HtmlRecommendationItem> Configure);

public sealed record HtmlRecommendationItem(string Title, string Detail, string? LinkTarget = null);

public static class HtmlReportViewModel
{
    public static HtmlRecommendationGroups BuildRecommendationGroups(
        IReadOnlyList<DiagnosticRecord> diagnostics,
        ScanMetrics metrics,
        QualityGateEvaluation? qualityGates,
        HtmlReportChecksRan checksRan,
        HtmlReportConfigHints configHints)
    {
        var fixNow = new List<HtmlRecommendationItem>();
        var nextSprint = new List<HtmlRecommendationItem>();
        var watch = new List<HtmlRecommendationItem>();
        var configure = new List<HtmlRecommendationItem>();

        foreach (var gate in qualityGates?.Gates.Where(static g => !g.Passed) ?? [])
        {
            var target = string.IsNullOrWhiteSpace(gate.Target) ? string.Empty : $" ({gate.Target})";
            fixNow.Add(new HtmlRecommendationItem(
                $"Quality gate failed: {gate.Name}{target}",
                $"Actual {gate.Actual}, limit {gate.Limit}. Reduce findings or adjust gate thresholds."));
        }

        foreach (var diagnostic in diagnostics.Where(static d => d.Severity == DiagnosticSeverity.Error))
        {
            fixNow.Add(new HtmlRecommendationItem(
                $"{diagnostic.Plugin}/{diagnostic.Rule}",
                diagnostic.Help.Length > 0 ? diagnostic.Help : diagnostic.Message,
                diagnostic.FilePath));
        }

        foreach (var diagnostic in diagnostics.Where(static d =>
            d.Rule.StartsWith("CSDS0001", StringComparison.OrdinalIgnoreCase)))
        {
            fixNow.Add(new HtmlRecommendationItem(
                "Vulnerable direct package",
                diagnostic.Help.Length > 0 ? diagnostic.Help : diagnostic.Message,
                diagnostic.FilePath));
        }

        foreach (var diagnostic in diagnostics.Where(static d =>
            d.Rule.StartsWith("CSDA0010", StringComparison.OrdinalIgnoreCase)
            || d.Rule.StartsWith("CSDA0011", StringComparison.OrdinalIgnoreCase)))
        {
            fixNow.Add(new HtmlRecommendationItem(
                "Architecture policy violation",
                diagnostic.Message,
                diagnostic.FilePath));
        }

        var hotRuleKeys = metrics.IssuesByRule
            .OrderByDescending(static pair => pair.Value.DebtScore)
            .Take(5)
            .Select(static pair => pair.Key)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var diagnostic in diagnostics.Where(d =>
            d.Severity == DiagnosticSeverity.Warning
            && hotRuleKeys.Contains($"{d.Plugin}/{d.Rule}")))
        {
            nextSprint.Add(new HtmlRecommendationItem(
                $"{diagnostic.Plugin}/{diagnostic.Rule}",
                diagnostic.Help.Length > 0 ? diagnostic.Help : diagnostic.Message,
                diagnostic.FilePath));
        }

        foreach (var diagnostic in diagnostics.Where(static d => d.Severity == DiagnosticSeverity.Info))
        {
            watch.Add(new HtmlRecommendationItem(
                $"{diagnostic.Plugin}/{diagnostic.Rule}",
                diagnostic.Message,
                diagnostic.FilePath));
        }

        if (configHints.QualityGatesDisabled)
        {
            configure.Add(new HtmlRecommendationItem(
                "Enable quality gates",
                "Set qualityGates.enabled to true in .csharp-doctor.json to enforce aggregate CI thresholds."));
        }

        if (configHints.NoBaseline)
        {
            configure.Add(new HtmlRecommendationItem(
                "Add a baseline",
                "Save a JSON report and set qualityGates.baselinePath to gate on new issues only."));
        }

        if (!checksRan.Architecture)
        {
            configure.Add(new HtmlRecommendationItem(
                "Run architecture checks",
                "Pass --architecture or checks.architecture to surface cycles, dead code, and policy violations."));
        }

        if (!checksRan.Sca)
        {
            configure.Add(new HtmlRecommendationItem(
                "Run SCA scan",
                "Pass --sca or checks.sca to audit NuGet packages for known vulnerabilities."));
        }

        if (configHints.NoArchitecturePolicy)
        {
            configure.Add(new HtmlRecommendationItem(
                "Define architecture policy",
                "Add architecturePolicy forbidden dependencies for UI/DAL or layer rules."));
        }

        return new HtmlRecommendationGroups(
            Deduplicate(fixNow),
            Deduplicate(nextSprint).Take(15).ToList(),
            Deduplicate(watch).Take(20).ToList(),
            Deduplicate(configure));
    }

    private static List<HtmlRecommendationItem> Deduplicate(IEnumerable<HtmlRecommendationItem> items) =>
        items
            .GroupBy(static item => item.Title + item.Detail, StringComparer.Ordinal)
            .Select(static group => group.First())
            .ToList();
}
