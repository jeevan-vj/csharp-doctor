using CSharpDoctor.Core.Diagnostics;
using CSharpDoctor.Ef.Migrations;
using DiagnosticSeverity = CSharpDoctor.Core.Diagnostics.DiagnosticSeverity;

namespace CSharpDoctor.Ef.Rules;

/// <summary>CSDE0002: non-nullable column added without default or backfill.</summary>
public static class Csde0002AddNotNullNoDefault
{
    public const string RuleId = "CSDE0002";

    public static IEnumerable<DiagnosticRecord> Run(IEnumerable<MigrationOperation> operations)
    {
        foreach (var operation in operations)
        {
            if (operation.Kind != "AddColumn")
            {
                continue;
            }

            if (operation.Nullable != false)
            {
                continue;
            }

            if (!string.IsNullOrWhiteSpace(operation.DefaultValue)
                || !string.IsNullOrWhiteSpace(operation.DefaultValueSql))
            {
                continue;
            }

            yield return Create(operation, operation.Column ?? "<unknown>", operation.Table ?? "<unknown>");
        }
    }

    private static DiagnosticRecord Create(MigrationOperation operation, string column, string table)
    {
        var lineSpan = operation.Location.GetLineSpan();
        return new DiagnosticRecord(
            FilePath: operation.Location.SourceTree?.FilePath ?? string.Empty,
            Plugin: "csharp-doctor",
            Rule: RuleId,
            Severity: DiagnosticSeverity.Warning,
            Message: $"Migration adds NOT NULL column '{column}' on '{table}' without defaultValue or defaultValueSql.",
            Help: "Add a default, backfill with Sql(), or add the column as nullable first and enforce NOT NULL in a later migration.",
            Line: lineSpan.StartLinePosition.Line + 1,
            Column: lineSpan.StartLinePosition.Character + 1,
            Category: "EfMigration",
            Tags: ["solution-scoped", "deploy-risk"]);
    }
}
