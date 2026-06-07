using CSharpDoctor.Architecture.Graph;
using CSharpDoctor.Core.Diagnostics;
using DiagnosticSeverity = CSharpDoctor.Core.Diagnostics.DiagnosticSeverity;

namespace CSharpDoctor.Architecture.Rules;

public static class Csda0002MutualNamespaceDependency
{
    public const string RuleId = "CSDA0002";

    public static IEnumerable<DiagnosticRecord> Run(NamespaceDependencyGraph graph)
    {
        foreach (var (first, second) in NamespaceDependencyGraph.FindMutualDependencies(graph))
        {
            yield return new DiagnosticRecord(
                FilePath: string.Empty,
                Plugin: "csharp-doctor",
                Rule: RuleId,
                Severity: DiagnosticSeverity.Warning,
                Message: $"Namespaces '{first}' and '{second}' are mutually dependent",
                Help: "Extract shared contracts to a neutral namespace to remove mutual dependencies.",
                Line: 0,
                Column: 0,
                Category: "Architecture",
                Tags: ["solution-scoped", "architecture"]);
        }
    }
}
