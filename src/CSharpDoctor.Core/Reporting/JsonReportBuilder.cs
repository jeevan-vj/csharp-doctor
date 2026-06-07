using System.Text.Json;
using System.Text.Json.Serialization;
using CSharpDoctor.Core.Diagnostics;
using CSharpDoctor.Core.Inspection;

namespace CSharpDoctor.Core.Reporting;

/// <summary>Builds versioned JSON inspection reports.</summary>
public static class JsonReportBuilder
{
    public const int SchemaVersion = 2;

    public static string Build(InspectResult result, InspectMode mode)
    {
        var report = new JsonReportV2(
            SchemaVersion,
            Ok: result.ExitCode == 0,
            Mode: mode.ToString().ToLowerInvariant(),
            Summary: new JsonReportSummary(result.ErrorCount, result.WarningCount, result.Diagnostics.Count),
            Metrics: result.Metrics is null ? null : ToJsonMetrics(result.Metrics),
            QualityGates: result.QualityGates is null ? null : ToJsonQualityGates(result.QualityGates),
            HotSpots: result.HotSpots is null ? null : ToJsonHotSpots(result.HotSpots),
            ElapsedMilliseconds: (long)result.Elapsed.TotalMilliseconds,
            Diagnostics: result.Diagnostics.Select(ToJsonDiagnostic).ToList(),
            Inspectors: BuildInspectors(result.InspectorRuns));

        return JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true });
    }

    private static IReadOnlyDictionary<string, JsonInspectorRun>? BuildInspectors(
        IReadOnlyDictionary<string, InspectorRunInfo>? runs) =>
        runs?.ToDictionary(
            static r => r.Key,
            static r => new JsonInspectorRun(r.Value.Ran, (long)r.Value.Elapsed.TotalMilliseconds, r.Value.DiagnosticCount));

    private static JsonMetrics ToJsonMetrics(ScanMetrics metrics) =>
        new(
            metrics.Errors,
            metrics.Warnings,
            metrics.Info,
            metrics.Total,
            metrics.DebtScore,
            metrics.IssuesByCategory.ToDictionary(
                static pair => pair.Key,
                static pair => new JsonMetricBucket(pair.Value.Count, pair.Value.DebtScore),
                StringComparer.OrdinalIgnoreCase));

    private static JsonQualityGates ToJsonQualityGates(QualityGateEvaluation evaluation) =>
        new(
            evaluation.Enabled,
            evaluation.Passed,
            evaluation.Gates.Select(static gate => new JsonQualityGate(
                gate.Name,
                gate.Limit,
                gate.Actual,
                gate.Passed,
                gate.Target)).ToList());

    private static JsonHotSpots ToJsonHotSpots(HotSpotsReport report) =>
        new(
            report.IssuesByRule.Select(ToJsonHotSpot).ToList(),
            report.IssuesByFile.Select(ToJsonHotSpot).ToList(),
            report.FixPriority.Select(ToJsonHotSpot).ToList());

    private static JsonHotSpot ToJsonHotSpot(HotSpotEntry entry) =>
        new(entry.Key, entry.Kind, entry.Count, entry.DebtScore, entry.PriorityScore);

    private static JsonDiagnostic ToJsonDiagnostic(DiagnosticRecord d) =>
        new(
            d.FilePath,
            $"{d.Plugin}/{d.Rule}",
            d.Severity.ToString().ToLowerInvariant(),
            d.Message,
            d.Help,
            d.Line,
            d.Column,
            d.Category,
            DiagnosticIdentity.Build(d));
}

public sealed record JsonReportV2(
    [property: JsonPropertyName("schemaVersion")] int SchemaVersion,
    bool Ok,
    string Mode,
    JsonReportSummary Summary,
    JsonMetrics? Metrics,
    JsonQualityGates? QualityGates,
    JsonHotSpots? HotSpots,
    long ElapsedMilliseconds,
    IReadOnlyList<JsonDiagnostic> Diagnostics,
    IReadOnlyDictionary<string, JsonInspectorRun>? Inspectors = null);

public sealed record JsonInspectorRun(bool Ran, long ElapsedMilliseconds, int Count);

public sealed record JsonReportSummary(int Errors, int Warnings, int Total);

public sealed record JsonMetrics(
    int Errors,
    int Warnings,
    int Info,
    int Total,
    int DebtScore,
    IReadOnlyDictionary<string, JsonMetricBucket> ByCategory);

public sealed record JsonMetricBucket(int Count, int DebtScore);

public sealed record JsonQualityGates(bool Enabled, bool Passed, IReadOnlyList<JsonQualityGate> Gates);

public sealed record JsonQualityGate(
    string Name,
    int? Limit,
    int Actual,
    bool Passed,
    string? Target = null);

public sealed record JsonHotSpots(
    IReadOnlyList<JsonHotSpot> IssuesByRule,
    IReadOnlyList<JsonHotSpot> IssuesByFile,
    IReadOnlyList<JsonHotSpot> FixPriority);

public sealed record JsonHotSpot(
    string Key,
    string Kind,
    int Count,
    int DebtScore,
    int PriorityScore);

public sealed record JsonDiagnostic(
    string FilePath,
    string Rule,
    string Severity,
    string Message,
    string Help,
    int Line,
    int Column,
    string Category,
    string Identity);
