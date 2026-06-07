using CSharpDoctor.Core.Diagnostics;
using CSharpDoctor.Ef.Migrations;
using DiagnosticSeverity = CSharpDoctor.Core.Diagnostics.DiagnosticSeverity;

namespace CSharpDoctor.Ef.Rules;

/// <summary>
/// CSDE0001: a migration drops a column, which breaks zero-downtime deploys if the old code is
/// still reading it during rollout.
/// </summary>
public static class Csde0001DropColumn
{
    public const string RuleId = "CSDE0001";

    public static IEnumerable<DiagnosticRecord> Run(IEnumerable<MigrationOperation> operations)
    {
        foreach (var operation in operations)
        {
            if (operation.Kind != "DropColumn")
            {
                continue;
            }

            var column = operation.Column ?? "<unknown>";
            var table = operation.Table ?? "<unknown>";
            var lineSpan = operation.Location.GetLineSpan();

            yield return new DiagnosticRecord(
                FilePath: operation.Location.SourceTree?.FilePath ?? string.Empty,
                Plugin: "csharp-doctor",
                Rule: RuleId,
                Severity: DiagnosticSeverity.Warning,
                Message: $"Migration drops column '{column}' from '{table}'. Verify the zero-downtime deploy path.",
                Help: "Deploy code that no longer reads the column first, then drop it in a later migration.",
                Line: lineSpan.StartLinePosition.Line + 1,
                Column: lineSpan.StartLinePosition.Character + 1,
                Category: "EfMigration",
                Tags: ["solution-scoped", "deploy-risk"]);
        }
    }
}
