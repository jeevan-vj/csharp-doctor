using CSharpDoctor.Core.Diagnostics;
using DiagnosticSeverity = CSharpDoctor.Core.Diagnostics.DiagnosticSeverity;

namespace CSharpDoctor.Sca.Rules;

public static class Csds0001VulnerableDirectPackage
{
    public const string RuleId = "CSDS0001";

    public static IEnumerable<DiagnosticRecord> Run(IEnumerable<VulnerablePackage> packages)
    {
        foreach (var package in packages.Where(static p => !p.IsTransitive))
        {
            var message =
                $"Vulnerable direct package '{package.PackageId}' {package.ResolvedVersion} ({package.Severity}).";
            yield return new DiagnosticRecord(
                FilePath: package.ProjectPath,
                Plugin: "csharp-doctor",
                Rule: RuleId,
                Severity: MapSeverity(package.Severity),
                Message: message,
                Help: $"Update or replace '{package.PackageId}'. Advisory: {package.AdvisoryUrl}",
                Line: 0,
                Column: 0,
                Category: "Security",
                Tags: ["solution-scoped", "sca", "dependency"]);
        }
    }

    private static DiagnosticSeverity MapSeverity(string severity) =>
        severity.Equals("High", StringComparison.OrdinalIgnoreCase)
        || severity.Equals("Critical", StringComparison.OrdinalIgnoreCase)
            ? DiagnosticSeverity.Error
            : DiagnosticSeverity.Warning;
}
