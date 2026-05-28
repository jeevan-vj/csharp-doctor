using System.Text;
using CSharpDoctor.Core.Diagnostics;

namespace CSharpDoctor.Core.Reporting;

/// <summary>Writes GitHub Actions workflow command annotations.</summary>
public static class GitHubAnnotationWriter
{
    public static string Render(IReadOnlyList<DiagnosticRecord> diagnostics)
    {
        var builder = new StringBuilder();
        foreach (var diagnostic in diagnostics)
        {
            var level = diagnostic.Severity == DiagnosticSeverity.Error ? "error" : "warning";
            var file = diagnostic.FilePath.Replace('%', '%');
            var message = Escape(diagnostic.Message);
            var title = $"{diagnostic.Plugin}/{diagnostic.Rule}";
            builder.AppendLine(
                $"::{level} file={file},line={diagnostic.Line},col={diagnostic.Column},title={title}::{message}");
        }

        return builder.ToString();
    }

    private static string Escape(string value) =>
        value.Replace("%", "%25", StringComparison.Ordinal)
            .Replace("\r", "%0D", StringComparison.Ordinal)
            .Replace("\n", "%0A", StringComparison.Ordinal);
}
