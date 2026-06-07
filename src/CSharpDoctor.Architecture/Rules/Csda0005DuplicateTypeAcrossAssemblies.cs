using CSharpDoctor.Core.Diagnostics;
using Microsoft.CodeAnalysis;
using DiagnosticSeverity = CSharpDoctor.Core.Diagnostics.DiagnosticSeverity;

namespace CSharpDoctor.Architecture.Rules;

public static class Csda0005DuplicateTypeAcrossAssemblies
{
    public const string RuleId = "CSDA0005";

    public static IEnumerable<DiagnosticRecord> Run(Solution solution, CancellationToken cancellationToken)
    {
        var byName = new Dictionary<string, List<INamedTypeSymbol>>(StringComparer.Ordinal);

        foreach (var project in solution.Projects)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var compilation = project.GetCompilationAsync(cancellationToken).GetAwaiter().GetResult();
            if (compilation is null)
            {
                continue;
            }

            foreach (var type in GetTypes(compilation.Assembly.GlobalNamespace))
            {
                var key = type.ToDisplayString();
                if (!byName.TryGetValue(key, out var list))
                {
                    list = [];
                    byName[key] = list;
                }

                list.Add(type);
            }
        }

        foreach (var (name, types) in byName)
        {
            var assemblies = types
                .Select(t => t.ContainingAssembly.Name)
                .Distinct(StringComparer.Ordinal)
                .ToList();

            if (assemblies.Count <= 1)
            {
                continue;
            }

            var first = types[0];
            var location = first.Locations.FirstOrDefault();
            var lineSpan = location?.GetLineSpan();
            yield return new DiagnosticRecord(
                FilePath: location?.SourceTree?.FilePath ?? string.Empty,
                Plugin: "csharp-doctor",
                Rule: RuleId,
                Severity: DiagnosticSeverity.Warning,
                Message: $"Type '{name}' is defined in multiple assemblies: {string.Join(", ", assemblies)}",
                Help: "Avoid duplicating type definitions; share a single contract assembly.",
                Line: (lineSpan?.StartLinePosition.Line ?? 0) + 1,
                Column: (lineSpan?.StartLinePosition.Character ?? 0) + 1,
                Category: "Architecture",
                Tags: ["solution-scoped", "architecture"]);
        }
    }

    private static IEnumerable<INamedTypeSymbol> GetTypes(INamespaceOrTypeSymbol symbol)
    {
        if (symbol is INamedTypeSymbol named)
        {
            yield return named;
        }

        foreach (var member in symbol.GetMembers())
        {
            if (member is INamespaceOrTypeSymbol child)
            {
                foreach (var type in GetTypes(child))
                {
                    yield return type;
                }
            }
        }
    }
}
