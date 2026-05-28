using System.Text.Json;
using System.Text.Json.Serialization;
using CSharpDoctor.Core.Diagnostics;
using CSharpDoctor.Core.Inspection;

namespace CSharpDoctor.Core.Reporting;

/// <summary>Builds versioned JSON inspection reports.</summary>
public static class JsonReportBuilder
{
    public const int SchemaVersion = 1;

    public static string Build(InspectResult result, InspectMode mode)
    {
        var report = new JsonReportV1(
            SchemaVersion,
            Ok: result.ExitCode == 0,
            Mode: mode.ToString().ToLowerInvariant(),
            Summary: new JsonReportSummary(result.ErrorCount, result.WarningCount, result.Diagnostics.Count),
            ElapsedMilliseconds: (long)result.Elapsed.TotalMilliseconds,
            Diagnostics: result.Diagnostics.Select(ToJsonDiagnostic).ToList());

        return JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true });
    }

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

public sealed record JsonReportV1(
    [property: JsonPropertyName("schemaVersion")] int SchemaVersion,
    bool Ok,
    string Mode,
    JsonReportSummary Summary,
    long ElapsedMilliseconds,
    IReadOnlyList<JsonDiagnostic> Diagnostics);

public sealed record JsonReportSummary(int Errors, int Warnings, int Total);

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
