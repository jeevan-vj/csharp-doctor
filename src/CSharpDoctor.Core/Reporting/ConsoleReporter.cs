using System.Text;
using CSharpDoctor.Core.Diagnostics;
using CSharpDoctor.Core.Inspection;

namespace CSharpDoctor.Core.Reporting;

/// <summary>Renders diagnostics for terminal output.</summary>
public static class ConsoleReporter
{
    public static string Render(
        IReadOnlyList<DiagnosticRecord> diagnostics,
        TimeSpan elapsed,
        IReadOnlyDictionary<string, InspectorRunInfo>? inspectorRuns = null,
        ScanMetrics? metrics = null,
        QualityGateEvaluation? qualityGates = null,
        HotSpotsReport? hotSpots = null)
    {
        var builder = new StringBuilder();
        builder.AppendLine("CSharp Doctor");
        builder.AppendLine($"Completed in {elapsed.TotalMilliseconds:F0}ms");
        AppendInspectorBreakdown(builder, inspectorRuns);
        AppendMetricsSummary(builder, metrics, qualityGates);
        builder.AppendLine();

        foreach (var group in diagnostics.GroupBy(static d => d.Category).OrderBy(static g => g.Key))
        {
            builder.AppendLine($"[{group.Key}]");
            foreach (var diagnostic in group.OrderByDescending(static d => d.Severity).ThenBy(static d => d.FilePath))
            {
                builder.AppendLine(
                    $"  {diagnostic.Severity,-7} {diagnostic.Plugin}/{diagnostic.Rule}  {diagnostic.FilePath}:{diagnostic.Line}:{diagnostic.Column}");
                builder.AppendLine($"           {diagnostic.Message}");
                if (!string.IsNullOrWhiteSpace(diagnostic.Help))
                {
                    builder.AppendLine($"           → {diagnostic.Help}");
                }

                if (!string.IsNullOrWhiteSpace(diagnostic.SuppressionHint))
                {
                    builder.AppendLine($"           hint: {diagnostic.SuppressionHint}");
                }
            }

            builder.AppendLine();
        }

        if (diagnostics.Count == 0)
        {
            builder.AppendLine("No issues found.");
        }

        AppendHotSpots(builder, hotSpots);
        return builder.ToString();
    }

    private static void AppendMetricsSummary(
        StringBuilder builder,
        ScanMetrics? metrics,
        QualityGateEvaluation? qualityGates)
    {
        if (metrics is null)
        {
            return;
        }

        builder.AppendLine(
            $"Metrics: {metrics.Errors} errors, {metrics.Warnings} warnings, debt {metrics.DebtScore}");

        if (qualityGates is not { Enabled: true })
        {
            return;
        }

        builder.AppendLine(qualityGates.Passed
            ? "Quality gates: PASSED"
            : "Quality gates: FAILED");
        foreach (var gate in qualityGates.Gates.Where(static g => !g.Passed))
        {
            var target = string.IsNullOrWhiteSpace(gate.Target) ? string.Empty : $" ({gate.Target})";
            builder.AppendLine($"  FAIL {gate.Name}{target}: {gate.Actual} / {gate.Limit}");
        }
    }

    private static void AppendHotSpots(StringBuilder builder, HotSpotsReport? hotSpots)
    {
        if (hotSpots is null || hotSpots.FixPriority.Count == 0)
        {
            return;
        }

        builder.AppendLine();
        builder.AppendLine("Fix priority (hot spots):");
        foreach (var entry in hotSpots.FixPriority.Take(5))
        {
            builder.AppendLine(
                $"  {entry.Kind,-4} {entry.Key}  count={entry.Count}  debt={entry.DebtScore}  priority={entry.PriorityScore}");
        }
    }

    private static void AppendInspectorBreakdown(
        StringBuilder builder,
        IReadOnlyDictionary<string, InspectorRunInfo>? inspectorRuns)
    {
        if (inspectorRuns is null)
        {
            return;
        }

        var ran = inspectorRuns.Where(static r => r.Value.Ran).ToList();
        if (ran.Count <= 1)
        {
            return;
        }

        builder.AppendLine("Inspectors:");
        foreach (var (name, info) in ran.OrderBy(static r => r.Key, StringComparer.Ordinal))
        {
            builder.AppendLine($"  {name,-8} {info.DiagnosticCount} findings  {info.Elapsed.TotalMilliseconds:F0}ms");
        }
    }
}
