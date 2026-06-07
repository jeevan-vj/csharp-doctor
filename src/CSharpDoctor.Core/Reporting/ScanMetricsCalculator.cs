using CSharpDoctor.Core.Configuration;
using CSharpDoctor.Core.Diagnostics;

namespace CSharpDoctor.Core.Reporting;

public static class ScanMetricsCalculator
{
    public static ScanMetrics Compute(
        IReadOnlyList<DiagnosticRecord> diagnostics,
        DebtWeightsConfig weights)
    {
        var errors = 0;
        var warnings = 0;
        var info = 0;
        var debtScore = 0;
        var byRule = new Dictionary<string, MutableBucket>(StringComparer.OrdinalIgnoreCase);
        var byFile = new Dictionary<string, MutableBucket>(StringComparer.OrdinalIgnoreCase);
        var byCategory = new Dictionary<string, MutableBucket>(StringComparer.OrdinalIgnoreCase);

        foreach (var diagnostic in diagnostics)
        {
            var weight = Weight(diagnostic.Severity, weights);
            debtScore += weight;

            switch (diagnostic.Severity)
            {
                case DiagnosticSeverity.Error:
                    errors++;
                    break;
                case DiagnosticSeverity.Warning:
                    warnings++;
                    break;
                case DiagnosticSeverity.Info:
                    info++;
                    break;
            }

            var ruleKey = $"{diagnostic.Plugin}/{diagnostic.Rule}";
            var fileKey = string.IsNullOrWhiteSpace(diagnostic.FilePath) ? "(solution)" : diagnostic.FilePath;
            Add(byRule, ruleKey, diagnostic.Severity, weight);
            Add(byFile, fileKey, diagnostic.Severity, weight);
            Add(byCategory, diagnostic.Category, diagnostic.Severity, weight);
        }

        return new ScanMetrics(
            errors,
            warnings,
            info,
            diagnostics.Count,
            debtScore,
            Freeze(byRule),
            Freeze(byFile),
            Freeze(byCategory));
    }

    private static int Weight(DiagnosticSeverity severity, DebtWeightsConfig weights) =>
        severity switch
        {
            DiagnosticSeverity.Error => weights.Error,
            DiagnosticSeverity.Warning => weights.Warning,
            DiagnosticSeverity.Info => weights.Info,
            _ => 0,
        };

    private static void Add(
        Dictionary<string, MutableBucket> buckets,
        string key,
        DiagnosticSeverity severity,
        int weight)
    {
        if (!buckets.TryGetValue(key, out var bucket))
        {
            bucket = new MutableBucket(key);
            buckets[key] = bucket;
        }

        bucket.Count++;
        bucket.DebtScore += weight;
        if (severity == DiagnosticSeverity.Error)
        {
            bucket.Errors++;
        }
        else if (severity == DiagnosticSeverity.Warning)
        {
            bucket.Warnings++;
        }
    }

    private static IReadOnlyDictionary<string, MetricBucket> Freeze(Dictionary<string, MutableBucket> buckets) =>
        buckets.ToDictionary(
            static pair => pair.Key,
            static pair => new MetricBucket(
                pair.Value.Key,
                pair.Value.Count,
                pair.Value.DebtScore,
                pair.Value.Errors,
                pair.Value.Warnings),
            StringComparer.OrdinalIgnoreCase);

    private sealed class MutableBucket(string key)
    {
        public string Key { get; } = key;

        public int Count { get; set; }

        public int DebtScore { get; set; }

        public int Errors { get; set; }

        public int Warnings { get; set; }
    }
}
