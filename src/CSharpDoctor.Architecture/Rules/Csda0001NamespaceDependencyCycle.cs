using CSharpDoctor.Architecture.Graph;
using CSharpDoctor.Core.Diagnostics;
using DiagnosticSeverity = CSharpDoctor.Core.Diagnostics.DiagnosticSeverity;

namespace CSharpDoctor.Architecture.Rules;

public static class Csda0001NamespaceDependencyCycle
{
    public const string RuleId = "CSDA0001";

    public static IEnumerable<DiagnosticRecord> Run(NamespaceDependencyGraph graph)
    {
        foreach (var cycle in NamespaceDependencyGraph.FindCycles(graph))
        {
            var message = "Namespace dependency cycle: " + string.Join(" -> ", cycle);
            yield return new DiagnosticRecord(
                FilePath: string.Empty,
                Plugin: "csharp-doctor",
                Rule: RuleId,
                Severity: DiagnosticSeverity.Warning,
                Message: message,
                Help: "Break the cycle by introducing abstractions or moving shared types to a lower-level namespace.",
                Line: 0,
                Column: 0,
                Category: "Architecture",
                Tags: ["solution-scoped", "architecture"]);
        }
    }
}
