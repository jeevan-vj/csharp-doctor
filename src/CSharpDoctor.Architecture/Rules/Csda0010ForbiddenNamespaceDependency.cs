using CSharpDoctor.Architecture.Graph;
using CSharpDoctor.Architecture.Policy;
using CSharpDoctor.Core.Configuration;
using CSharpDoctor.Core.Diagnostics;
using DiagnosticSeverity = CSharpDoctor.Core.Diagnostics.DiagnosticSeverity;

namespace CSharpDoctor.Architecture.Rules;

public static class Csda0010ForbiddenNamespaceDependency
{
    public const string RuleId = "CSDA0010";

    public static IEnumerable<DiagnosticRecord> Run(
        NamespaceDependencyGraph graph,
        ArchitecturePolicyConfig policy)
    {
        if (policy.ForbiddenNamespaceDependencies.Count == 0)
        {
            yield break;
        }

        foreach (var (fromNamespace, targets) in graph.Edges)
        {
            foreach (var toNamespace in targets)
            {
                foreach (var rule in policy.ForbiddenNamespaceDependencies)
                {
                    if (!DependencyPatternMatcher.MatchesNamespace(rule.From, fromNamespace)
                        || !DependencyPatternMatcher.MatchesNamespace(rule.To, toNamespace))
                    {
                        continue;
                    }

                    var message =
                        $"Forbidden namespace dependency: '{fromNamespace}' must not use '{toNamespace}' (policy {rule.From} -> {rule.To}).";
                    yield return new DiagnosticRecord(
                        FilePath: string.Empty,
                        Plugin: "csharp-doctor",
                        Rule: RuleId,
                        Severity: DiagnosticSeverity.Warning,
                        Message: message,
                        Help: "Introduce abstractions or move shared contracts to a layer both sides may reference.",
                        Line: 0,
                        Column: 0,
                        Category: "Architecture",
                        Tags: ["solution-scoped", "architecture", "policy"]);
                }
            }
        }
    }
}
