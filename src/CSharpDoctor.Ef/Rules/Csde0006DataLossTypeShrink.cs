using CSharpDoctor.Core.Diagnostics;
using CSharpDoctor.Ef.Migrations;
using DiagnosticSeverity = CSharpDoctor.Core.Diagnostics.DiagnosticSeverity;

namespace CSharpDoctor.Ef.Rules;

/// <summary>CSDE0006: column type shrink that can cause data loss.</summary>
public static class Csde0006DataLossTypeShrink
{
    public const string RuleId = "CSDE0006";

    public static IEnumerable<DiagnosticRecord> Run(IEnumerable<MigrationOperation> operations)
    {
        foreach (var operation in operations)
        {
            if (operation.Kind != "AlterColumn")
            {
                continue;
            }

            if (!TypeShrinkAnalyzer.IsLikelyDataLoss(operation.OldColumnType, operation.ColumnType))
            {
                continue;
            }

            var lineSpan = operation.Location.GetLineSpan();
            yield return new DiagnosticRecord(
                FilePath: operation.Location.SourceTree?.FilePath ?? string.Empty,
                Plugin: "csharp-doctor",
                Rule: RuleId,
                Severity: DiagnosticSeverity.Warning,
                Message: $"Migration shrinks column '{operation.Column ?? "<unknown>"}' type from '{operation.OldColumnType ?? "?"}' to '{operation.ColumnType ?? "?"}'.",
                Help: "Verify existing rows fit the new type or migrate data before narrowing the column.",
                Line: lineSpan.StartLinePosition.Line + 1,
                Column: lineSpan.StartLinePosition.Character + 1,
                Category: "EfMigration",
                Tags: ["solution-scoped", "deploy-risk"]);
        }
    }
}
