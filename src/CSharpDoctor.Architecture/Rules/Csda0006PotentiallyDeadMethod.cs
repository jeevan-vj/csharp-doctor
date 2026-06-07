using CSharpDoctor.Architecture.Graph;
using CSharpDoctor.Core.Diagnostics;
using Microsoft.CodeAnalysis;
using DiagnosticSeverity = CSharpDoctor.Core.Diagnostics.DiagnosticSeverity;

namespace CSharpDoctor.Architecture.Rules;

public static class Csda0006PotentiallyDeadMethod
{
    public const string RuleId = "CSDA0006";

    public static IEnumerable<DiagnosticRecord> Run(SolutionReferenceIndex index, CancellationToken cancellationToken)
    {
        foreach (var (symbol, count) in index.ReferenceCounts)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (symbol is not IMethodSymbol method)
            {
                continue;
            }

            if (method.MethodKind is not (MethodKind.Ordinary or MethodKind.LocalFunction))
            {
                continue;
            }

            if (method.DeclaredAccessibility is not (Accessibility.Private or Accessibility.Internal))
            {
                continue;
            }

            if (method.IsOverride || method.IsAbstract || method.IsImplicitlyDeclared)
            {
                continue;
            }

            if (method.Name is "Main" or "Dispose" or "GetEnumerator")
            {
                continue;
            }

            if (count > 1)
            {
                continue;
            }

            var location = method.Locations.FirstOrDefault();
            var lineSpan = location?.GetLineSpan();
            yield return new DiagnosticRecord(
                FilePath: location?.SourceTree?.FilePath ?? string.Empty,
                Plugin: "csharp-doctor",
                Rule: RuleId,
                Severity: DiagnosticSeverity.Info,
                Message: $"Method '{method.ToDisplayString()}' appears unused in the solution",
                Help: "Remove dead methods or expose them via tests/reflection intentionally.",
                Line: (lineSpan?.StartLinePosition.Line ?? 0) + 1,
                Column: (lineSpan?.StartLinePosition.Character ?? 0) + 1,
                Category: "Architecture",
                Tags: ["solution-scoped", "architecture", "design"]);
        }
    }
}
