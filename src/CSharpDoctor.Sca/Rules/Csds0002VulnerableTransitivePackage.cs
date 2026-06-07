using CSharpDoctor.Core.Diagnostics;
using DiagnosticSeverity = CSharpDoctor.Core.Diagnostics.DiagnosticSeverity;

namespace CSharpDoctor.Sca.Rules;

public static class Csds0002VulnerableTransitivePackage
{
    public const string RuleId = "CSDS0002";

    public static IEnumerable<DiagnosticRecord> Run(IEnumerable<VulnerablePackage> packages)
    {
        foreach (var package in packages.Where(static p => p.IsTransitive))
        {
            var message =
                $"Vulnerable transitive package '{package.PackageId}' {package.ResolvedVersion} ({package.Severity}).";
            yield return new DiagnosticRecord(
                FilePath: package.ProjectPath,
                Plugin: "csharp-doctor",
                Rule: RuleId,
                Severity: MapSeverity(package.Severity),
                Message: message,
                Help: $"Add a direct reference to a patched version or update parent packages. Advisory: {package.AdvisoryUrl}",
                Line: 0,
                Column: 0,
                Category: "Security",
                Tags: ["solution-scoped", "sca", "dependency"]);
        }
    }

    private static DiagnosticSeverity MapSeverity(string severity) =>
        severity.Equals("High", StringComparison.OrdinalIgnoreCase)
        || severity.Equals("Critical", StringComparison.OrdinalIgnoreCase)
            ? DiagnosticSeverity.Warning
            : DiagnosticSeverity.Info;
}
