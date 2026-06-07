using CSharpDoctor.Core.Diagnostics;
using CSharpDoctor.Wiring.IntentDiscovery;
using CSharpDoctor.Wiring.Registration;
using DiagnosticSeverity = CSharpDoctor.Core.Diagnostics.DiagnosticSeverity;

namespace CSharpDoctor.Wiring.Rules;

/// <summary>
/// CSDW0001: a concrete hosted service is declared but never registered via
/// <c>AddHostedService&lt;T&gt;()</c>, so it will silently never run.
/// </summary>
public static class Csdw0001UnregisteredHostedService
{
    public const string RuleId = "CSDW0001";

    public static IEnumerable<DiagnosticRecord> Run(
        IEnumerable<HostedServiceCandidate> candidates,
        RegistrationGraph registrations)
    {
        foreach (var candidate in candidates)
        {
            if (registrations.IsHostedServiceRegistered(candidate.TypeName))
            {
                continue;
            }

            var lineSpan = candidate.Location.GetLineSpan();

            yield return new DiagnosticRecord(
                FilePath: candidate.Location.SourceTree?.FilePath ?? string.Empty,
                Plugin: "csharp-doctor",
                Rule: RuleId,
                Severity: DiagnosticSeverity.Warning,
                Message: $"Hosted service '{candidate.TypeName}' is declared but never registered with AddHostedService<>().",
                Help: $"Register it in your composition root: services.AddHostedService<{candidate.TypeName}>();",
                Line: lineSpan.StartLinePosition.Line + 1,
                Column: lineSpan.StartLinePosition.Character + 1,
                Category: "Wiring",
                Tags: ["solution-scoped"]);
        }
    }
}
