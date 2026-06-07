using CSharpDoctor.Architecture.Graph;
using CSharpDoctor.Core.Diagnostics;
using DiagnosticSeverity = CSharpDoctor.Core.Diagnostics.DiagnosticSeverity;

namespace CSharpDoctor.Architecture.Rules;

public static class Csda0008MutuallyDependentTypes
{
    public const string RuleId = "CSDA0008";

    public static IEnumerable<DiagnosticRecord> Run(TypeDependencyGraph graph)
    {
        foreach (var (first, second) in TypeDependencyGraph.FindMutualDependencies(graph))
        {
            yield return new DiagnosticRecord(
                FilePath: string.Empty,
                Plugin: "csharp-doctor",
                Rule: RuleId,
                Severity: DiagnosticSeverity.Warning,
                Message: $"Types '{first}' and '{second}' are mutually dependent",
                Help: "Break mutual type dependencies with interfaces or moving shared contracts.",
                Line: 0,
                Column: 0,
                Category: "Architecture",
                Tags: ["solution-scoped", "architecture"]);
        }
    }
}
