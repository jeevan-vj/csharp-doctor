using CSharpDoctor.Architecture.Policy;
using CSharpDoctor.Core.Configuration;
using CSharpDoctor.Core.Diagnostics;
using Microsoft.CodeAnalysis;
using DiagnosticSeverity = CSharpDoctor.Core.Diagnostics.DiagnosticSeverity;

namespace CSharpDoctor.Architecture.Rules;

public static class Csda0011ForbiddenAssemblyReference
{
    public const string RuleId = "CSDA0011";

    public static IEnumerable<DiagnosticRecord> Run(Solution solution, ArchitecturePolicyConfig policy)
    {
        if (policy.ForbiddenAssemblyReferences.Count == 0)
        {
            yield break;
        }

        foreach (var project in solution.Projects)
        {
            var fromAssembly = project.AssemblyName ?? project.Name;
            foreach (var reference in project.ProjectReferences)
            {
                var referenced = solution.GetProject(reference.ProjectId);
                if (referenced is null)
                {
                    continue;
                }

                var toAssembly = referenced.AssemblyName ?? referenced.Name;
                foreach (var rule in policy.ForbiddenAssemblyReferences)
                {
                    if (!DependencyPatternMatcher.MatchesAssembly(rule.From, fromAssembly)
                        || !DependencyPatternMatcher.MatchesAssembly(rule.To, toAssembly))
                    {
                        continue;
                    }

                    var message =
                        $"Forbidden assembly reference: '{fromAssembly}' must not reference '{toAssembly}' (policy {rule.From} -> {rule.To}).";
                    yield return new DiagnosticRecord(
                        FilePath: project.FilePath ?? string.Empty,
                        Plugin: "csharp-doctor",
                        Rule: RuleId,
                        Severity: DiagnosticSeverity.Warning,
                        Message: message,
                        Help: "Remove the project reference or introduce an abstraction assembly both layers may use.",
                        Line: 0,
                        Column: 0,
                        Category: "Architecture",
                        Tags: ["solution-scoped", "architecture", "policy"]);
                }
            }
        }
    }
}
