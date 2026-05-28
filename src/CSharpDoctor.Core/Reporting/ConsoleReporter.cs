using System.Text;
using CSharpDoctor.Core.Diagnostics;

namespace CSharpDoctor.Core.Reporting;

/// <summary>Renders diagnostics for terminal output.</summary>
public static class ConsoleReporter
{
    public static string Render(IReadOnlyList<DiagnosticRecord> diagnostics, TimeSpan elapsed)
    {
        var builder = new StringBuilder();
        builder.AppendLine("CSharp Doctor");
        builder.AppendLine($"Completed in {elapsed.TotalMilliseconds:F0}ms");
        builder.AppendLine();

        foreach (var group in diagnostics.GroupBy(static d => d.Category).OrderBy(static g => g.Key))
        {
            builder.AppendLine($"[{group.Key}]");
            foreach (var diagnostic in group.OrderByDescending(static d => d.Severity).ThenBy(static d => d.FilePath))
            {
                builder.AppendLine(
                    $"  {diagnostic.Severity,-7} {diagnostic.Plugin}/{diagnostic.Rule}  {diagnostic.FilePath}:{diagnostic.Line}:{diagnostic.Column}");
                builder.AppendLine($"           {diagnostic.Message}");
                if (!string.IsNullOrWhiteSpace(diagnostic.Help))
                {
                    builder.AppendLine($"           → {diagnostic.Help}");
                }

                if (!string.IsNullOrWhiteSpace(diagnostic.SuppressionHint))
                {
                    builder.AppendLine($"           hint: {diagnostic.SuppressionHint}");
                }
            }

            builder.AppendLine();
        }

        if (diagnostics.Count == 0)
        {
            builder.AppendLine("No issues found.");
        }

        return builder.ToString();
    }
}
