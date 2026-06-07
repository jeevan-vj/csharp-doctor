using CSharpDoctor.Core.Configuration;

namespace CSharpDoctor.Core.Reporting;

public static class HotSpotRanker
{
    public static HotSpotsReport Rank(ScanMetrics metrics, HotSpotsConfig config)
    {
        var byRule = RankBuckets(metrics.IssuesByRule.Values, "rule", config.TopRules);
        var byFile = RankBuckets(metrics.IssuesByFile.Values, "file", config.TopFiles);
        var fixPriority = BuildFixPriority(byRule, byFile, config.TopElements);
        return new HotSpotsReport(byRule, byFile, fixPriority);
    }

    private static IReadOnlyList<HotSpotEntry> RankBuckets(
        IEnumerable<MetricBucket> buckets,
        string kind,
        int limit) =>
        buckets
            .Select(bucket => new HotSpotEntry(
                bucket.Key,
                kind,
                bucket.Count,
                bucket.DebtScore,
                PriorityScore(bucket.DebtScore, bucket.Count)))
            .OrderByDescending(static entry => entry.PriorityScore)
            .ThenByDescending(static entry => entry.DebtScore)
            .Take(limit)
            .ToList();

    private static IReadOnlyList<HotSpotEntry> BuildFixPriority(
        IReadOnlyList<HotSpotEntry> byRule,
        IReadOnlyList<HotSpotEntry> byFile,
        int limit) =>
        byRule
            .Concat(byFile)
            .OrderByDescending(static entry => entry.PriorityScore)
            .ThenByDescending(static entry => entry.DebtScore)
            .Take(limit)
            .ToList();

    private static int PriorityScore(int debtScore, int count) => debtScore * Math.Max(1, count);
}
