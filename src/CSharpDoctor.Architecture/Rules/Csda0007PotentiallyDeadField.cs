using CSharpDoctor.Architecture.Graph;
using CSharpDoctor.Core.Diagnostics;
using Microsoft.CodeAnalysis;
using DiagnosticSeverity = CSharpDoctor.Core.Diagnostics.DiagnosticSeverity;

namespace CSharpDoctor.Architecture.Rules;

public static class Csda0007PotentiallyDeadField
{
    public const string RuleId = "CSDA0007";

    public static IEnumerable<DiagnosticRecord> Run(SolutionReferenceIndex index, CancellationToken cancellationToken)
    {
        foreach (var (symbol, count) in index.ReferenceCounts)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (symbol is not IFieldSymbol field)
            {
                continue;
            }

            if (field.DeclaredAccessibility is not (Accessibility.Private or Accessibility.Internal))
            {
                continue;
            }

            if (field.IsImplicitlyDeclared || field.AssociatedSymbol is not null)
            {
                continue;
            }

            if (count > 1)
            {
                continue;
            }

            var location = field.Locations.FirstOrDefault();
            var lineSpan = location?.GetLineSpan();
            yield return new DiagnosticRecord(
                FilePath: location?.SourceTree?.FilePath ?? string.Empty,
                Plugin: "csharp-doctor",
                Rule: RuleId,
                Severity: DiagnosticSeverity.Info,
                Message: $"Field '{field.ToDisplayString()}' appears unused in the solution",
                Help: "Remove unused fields or mark them as intentionally retained.",
                Line: (lineSpan?.StartLinePosition.Line ?? 0) + 1,
                Column: (lineSpan?.StartLinePosition.Character ?? 0) + 1,
                Category: "Architecture",
                Tags: ["solution-scoped", "architecture", "design"]);
        }
    }
}
