using CSharpDoctor.Core.Diagnostics;
using CSharpDoctor.Wiring.IntentDiscovery;
using CSharpDoctor.Wiring.Registration;
using DiagnosticSeverity = CSharpDoctor.Core.Diagnostics.DiagnosticSeverity;

namespace CSharpDoctor.Wiring.Rules;

/// <summary>
/// CSDW0010: a singleton service directly depends on a scoped service (captive dependency).
/// </summary>
public static class Csdw0010CaptiveDependency
{
    public const string RuleId = "CSDW0010";

    public static IEnumerable<DiagnosticRecord> Run(
        IEnumerable<ConstructorDependency> dependencies,
        RegistrationGraph registrations)
    {
        foreach (var dependency in dependencies)
        {
            var containingLifetime = registrations.GetLifetime(dependency.ContainingTypeName);
            if (containingLifetime != ServiceLifetime.Singleton)
            {
                continue;
            }

            if (WiringTypeIdentity.IsFrameworkProvided(dependency.ParameterTypeName))
            {
                continue;
            }

            var depLifetime = registrations.GetLifetime(dependency.ParameterTypeName);
            if (depLifetime != ServiceLifetime.Scoped)
            {
                continue;
            }

            var lineSpan = dependency.Location.GetLineSpan();
            var paramName = WiringTypeIdentity.Normalize(dependency.ParameterTypeName);

            yield return new DiagnosticRecord(
                FilePath: dependency.Location.SourceTree?.FilePath ?? string.Empty,
                Plugin: "csharp-doctor",
                Rule: RuleId,
                Severity: DiagnosticSeverity.Warning,
                Message: $"Singleton '{dependency.ContainingTypeName}' depends on scoped service '{paramName}'.",
                Help: "Change the singleton to scoped, inject IServiceScopeFactory, or register the dependency as singleton.",
                Line: lineSpan.StartLinePosition.Line + 1,
                Column: lineSpan.StartLinePosition.Character + 1,
                Category: "Wiring",
                Tags: ["solution-scoped"]);
        }
    }
}
