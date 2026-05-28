using System.Collections.Immutable;
using Basic.Reference.Assemblies;
using CSharpDoctor.Core.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using CoreSeverity = CSharpDoctor.Core.Diagnostics.DiagnosticSeverity;

namespace CSharpDoctor.Core.Analysis;

/// <summary>Runs Roslyn analyzers against source files.</summary>
public static class AnalyzerRunner
{
    public static async Task<IReadOnlyList<DiagnosticRecord>> RunAsync(
        IEnumerable<string> sourceFilePaths,
        ImmutableArray<DiagnosticAnalyzer> analyzers,
        CancellationToken cancellationToken = default)
    {
        var trees = new List<SyntaxTree>();
        foreach (var path in sourceFilePaths.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!File.Exists(path) || !path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var text = await File.ReadAllTextAsync(path, cancellationToken).ConfigureAwait(false);
            var parseOptions = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp12);
            trees.Add(CSharpSyntaxTree.ParseText(text, parseOptions, path: path));
        }

        if (trees.Count == 0)
        {
            return [];
        }

        var references = Net80.References.All.Cast<MetadataReference>().ToArray();

        var compilation = CSharpCompilation.Create(
            "CSharpDoctorInspection",
            trees,
            references,
            new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary,
                nullableContextOptions: NullableContextOptions.Enable));

        var withAnalyzers = compilation.WithAnalyzers(analyzers);
        var diagnostics = await withAnalyzers.GetAnalyzerDiagnosticsAsync(cancellationToken)
            .ConfigureAwait(false);

        return diagnostics
            .Where(static d => d.Location.IsInSource)
            .Select(Map)
            .ToList();
    }

    private static DiagnosticRecord Map(Diagnostic diagnostic)
    {
        var lineSpan = diagnostic.Location.GetLineSpan();
        var line = lineSpan.StartLinePosition.Line + 1;
        var column = lineSpan.StartLinePosition.Character + 1;

        return new DiagnosticRecord(
            FilePath: diagnostic.Location.SourceTree?.FilePath ?? string.Empty,
            Plugin: "csharp-doctor",
            Rule: diagnostic.Id,
            Severity: MapSeverity(diagnostic.Severity),
            Message: diagnostic.GetMessage(),
            Help: diagnostic.Descriptor.Description.ToString(),
            Line: line,
            Column: column,
            Category: diagnostic.Descriptor.Category,
            Tags: diagnostic.Descriptor.CustomTags.ToArray());
    }

    private static CoreSeverity MapSeverity(Microsoft.CodeAnalysis.DiagnosticSeverity severity) =>
        severity switch
        {
            Microsoft.CodeAnalysis.DiagnosticSeverity.Error => CoreSeverity.Error,
            Microsoft.CodeAnalysis.DiagnosticSeverity.Warning => CoreSeverity.Warning,
            Microsoft.CodeAnalysis.DiagnosticSeverity.Info => CoreSeverity.Info,
            _ => CoreSeverity.Hidden,
        };
}
