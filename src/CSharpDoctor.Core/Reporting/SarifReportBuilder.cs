using System.Text.Json;
using CSharpDoctor.Core.Diagnostics;

namespace CSharpDoctor.Core.Reporting;

/// <summary>Builds SARIF 2.1.0 reports for code scanning integrations.</summary>
public static class SarifReportBuilder
{
    public static string Build(IReadOnlyList<DiagnosticRecord> diagnostics)
    {
        var runs = new[]
        {
            new SarifRun(
                Tool: new SarifTool(
                    Driver: new SarifDriver(
                        Name: "csharp-doctor",
                        InformationUri: "https://github.com/csharp-doctor/csharp-doctor",
                        Rules: diagnostics
                            .Select(static d => d.Rule)
                            .Distinct(StringComparer.OrdinalIgnoreCase)
                            .Select(ruleId => new SarifRule(ruleId, ruleId, "warning"))
                            .ToList())),
                Results: diagnostics.Select(ToResult).ToList()),
        };

        var log = new SarifLog("2.1.0", runs);
        return JsonSerializer.Serialize(log, new JsonSerializerOptions { WriteIndented = true });
    }

    private static SarifResult ToResult(DiagnosticRecord d) =>
        new(
            RuleId: d.Rule,
            Level: d.Severity switch
            {
                DiagnosticSeverity.Error => "error",
                DiagnosticSeverity.Warning => "warning",
                _ => "note",
            },
            Message: new SarifMessage(d.Message),
            Locations:
            [
                new SarifLocation(
                    PhysicalLocation: new SarifPhysicalLocation(
                        ArtifactLocation: new SarifArtifactLocation(d.FilePath),
                        Region: new SarifRegion(d.Line, d.Column))),
            ]);
}

public sealed record SarifLog(string Version, IReadOnlyList<SarifRun> Runs);

public sealed record SarifRun(SarifTool Tool, IReadOnlyList<SarifResult> Results);

public sealed record SarifTool(SarifDriver Driver);

public sealed record SarifDriver(string Name, string InformationUri, IReadOnlyList<SarifRule> Rules);

public sealed record SarifRule(string Id, string Name, string DefaultLevel);

public sealed record SarifResult(string RuleId, string Level, SarifMessage Message, IReadOnlyList<SarifLocation> Locations);

public sealed record SarifMessage(string Text);

public sealed record SarifLocation(SarifPhysicalLocation PhysicalLocation);

public sealed record SarifPhysicalLocation(SarifArtifactLocation ArtifactLocation, SarifRegion Region);

public sealed record SarifArtifactLocation(string Uri);

public sealed record SarifRegion(int StartLine, int StartColumn);
