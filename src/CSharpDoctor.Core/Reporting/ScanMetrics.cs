namespace CSharpDoctor.Core.Reporting;

public sealed record MetricBucket(string Key, int Count, int DebtScore, int Errors, int Warnings);

public sealed record ScanMetrics(
    int Errors,
    int Warnings,
    int Info,
    int Total,
    int DebtScore,
    IReadOnlyDictionary<string, MetricBucket> IssuesByRule,
    IReadOnlyDictionary<string, MetricBucket> IssuesByFile,
    IReadOnlyDictionary<string, MetricBucket> IssuesByCategory);

public sealed record QualityGateResult(
    string Name,
    int? Limit,
    int Actual,
    bool Passed,
    string? Target = null);

public sealed record QualityGateEvaluation(
    bool Enabled,
    bool Passed,
    IReadOnlyList<QualityGateResult> Gates);

public sealed record HotSpotEntry(
    string Key,
    string Kind,
    int Count,
    int DebtScore,
    int PriorityScore);

public sealed record HotSpotsReport(
    IReadOnlyList<HotSpotEntry> IssuesByRule,
    IReadOnlyList<HotSpotEntry> IssuesByFile,
    IReadOnlyList<HotSpotEntry> FixPriority);
