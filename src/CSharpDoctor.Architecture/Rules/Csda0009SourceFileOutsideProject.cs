using System.Xml.Linq;
using CSharpDoctor.Core.Diagnostics;
using DiagnosticSeverity = CSharpDoctor.Core.Diagnostics.DiagnosticSeverity;

namespace CSharpDoctor.Architecture.Rules;

public static class Csda0009SourceFileOutsideProject
{
    public const string RuleId = "CSDA0009";

    public static IEnumerable<DiagnosticRecord> Run(IReadOnlyList<string> csprojPaths, CancellationToken cancellationToken)
    {
        foreach (var csprojPath in csprojPaths)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!File.Exists(csprojPath))
            {
                continue;
            }

            var projectDir = Path.GetDirectoryName(csprojPath)!;
            var doc = XDocument.Load(csprojPath);
            var compileItems = doc.Descendants()
                .Where(e => e.Name.LocalName == "Compile")
                .Select(e => e.Attribute("Include")?.Value)
                .Where(v => !string.IsNullOrWhiteSpace(v));

            foreach (var include in compileItems)
            {
                if (!include!.Contains("..", StringComparison.Ordinal))
                {
                    continue;
                }

                var fullPath = Path.GetFullPath(Path.Combine(projectDir, include));
                yield return new DiagnosticRecord(
                    FilePath: fullPath,
                    Plugin: "csharp-doctor",
                    Rule: RuleId,
                    Severity: DiagnosticSeverity.Warning,
                    Message: $"Source file '{include}' is referenced outside the project directory",
                    Help: "Keep source files inside the project folder or use project/assembly references instead.",
                    Line: 0,
                    Column: 0,
                    Category: "Architecture",
                    Tags: ["solution-scoped", "architecture"]);
            }
        }
    }
}
