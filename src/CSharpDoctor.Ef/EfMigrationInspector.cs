using CSharpDoctor.Core.Diagnostics;
using CSharpDoctor.Core.Inspection;
using CSharpDoctor.Ef.Migrations;
using CSharpDoctor.Ef.Rules;
using Microsoft.CodeAnalysis.CSharp;

namespace CSharpDoctor.Ef;

/// <summary>
/// Scans EF Core migration files (under a <c>Migrations</c> folder, referencing
/// <c>MigrationBuilder</c>) for unsafe operations. Pure file/AST scan; no workspace required.
/// </summary>
public sealed class EfMigrationInspector : IInspector
{
    public string Name => InspectorRegistry.EfInspectorName;

    public async Task<IReadOnlyList<DiagnosticRecord>> InspectAsync(
        InspectContext context,
        CancellationToken cancellationToken = default)
    {
        var diagnostics = new List<DiagnosticRecord>();

        foreach (var file in context.SourceFiles)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!IsMigrationFile(file) || !File.Exists(file))
            {
                continue;
            }

            var text = await File.ReadAllTextAsync(file, cancellationToken).ConfigureAwait(false);
            if (!text.Contains("MigrationBuilder", StringComparison.Ordinal))
            {
                continue;
            }

            var tree = CSharpSyntaxTree.ParseText(text, path: file, cancellationToken: cancellationToken);
            var root = await tree.GetRootAsync(cancellationToken).ConfigureAwait(false);

            var operations = MigrationParser.Parse(root);
            diagnostics.AddRange(Csde0001DropColumn.Run(operations));
            diagnostics.AddRange(Csde0002AddNotNullNoDefault.Run(operations));
            diagnostics.AddRange(Csde0006DataLossTypeShrink.Run(operations));
        }

        return diagnostics;
    }

    private static bool IsMigrationFile(string path) =>
        path.Replace('\\', '/').Contains("/Migrations/", StringComparison.OrdinalIgnoreCase);
}
