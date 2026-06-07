using CSharpDoctor.Core.Diagnostics;
using CSharpDoctor.Wiring.IntentDiscovery;
using CSharpDoctor.Wiring.Registration;
using DiagnosticSeverity = CSharpDoctor.Core.Diagnostics.DiagnosticSeverity;

namespace CSharpDoctor.Wiring.Rules;

/// <summary>
/// CSDW0008: constructor asks for an interface/abstract service that is never registered in the composition root.
/// </summary>
public static class Csdw0008UnresolvedDependency
{
    public const string RuleId = "CSDW0008";

    public static IEnumerable<DiagnosticRecord> Run(
        IEnumerable<ConstructorDependency> dependencies,
        RegistrationGraph registrations)
    {
        foreach (var dependency in dependencies)
        {
            if (!dependency.IsInterfaceOrAbstract)
            {
                continue;
            }

            if (WiringTypeIdentity.IsFrameworkProvided(dependency.ParameterTypeName))
            {
                continue;
            }

            if (registrations.IsServiceRegistered(dependency.ParameterTypeName))
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
                Message: $"Constructor parameter '{paramName}' on '{dependency.ContainingTypeName}' is not registered in DI.",
                Help: $"Register the service in your composition root: services.AddScoped<{paramName}, ...>();",
                Line: lineSpan.StartLinePosition.Line + 1,
                Column: lineSpan.StartLinePosition.Character + 1,
                Category: "Wiring",
                Tags: ["solution-scoped"]);
        }
    }
}
