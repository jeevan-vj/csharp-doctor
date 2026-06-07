using CSharpDoctor.Core.Diagnostics;
using Microsoft.CodeAnalysis;
using DiagnosticSeverity = CSharpDoctor.Core.Diagnostics.DiagnosticSeverity;

namespace CSharpDoctor.Architecture.Rules;

public static class Csda0003PotentiallyDeadType
{
    public const string RuleId = "CSDA0003";

    public static IEnumerable<DiagnosticRecord> Run(Solution solution, CancellationToken cancellationToken)
    {
        var typeRefs = new Dictionary<INamedTypeSymbol, int>(SymbolEqualityComparer.Default);
        var allTypes = new List<INamedTypeSymbol>();

        foreach (var project in solution.Projects)
        {
            var compilation = project.GetCompilationAsync(cancellationToken).GetAwaiter().GetResult();
            if (compilation is null)
            {
                continue;
            }

            CollectTypes(compilation.Assembly.GlobalNamespace, allTypes);
        }

        foreach (var type in allTypes)
        {
            typeRefs[type] = 0;
        }

        foreach (var project in solution.Projects)
        {
            foreach (var document in project.Documents)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var model = document.GetSemanticModelAsync(cancellationToken).GetAwaiter().GetResult();
                var root = document.GetSyntaxRootAsync(cancellationToken).GetAwaiter().GetResult();
                if (model is null || root is null)
                {
                    continue;
                }

                foreach (var node in root.DescendantNodes())
                {
                    var symbol = model.GetSymbolInfo(node, cancellationToken).Symbol;
                    if (symbol is INamedTypeSymbol named && typeRefs.ContainsKey(named))
                    {
                        typeRefs[named]++;
                    }
                }
            }
        }

        foreach (var type in allTypes)
        {
            if (type.DeclaredAccessibility is not (Accessibility.Internal or Accessibility.Private))
            {
                continue;
            }

            if (type.Name is "Program" or "<Module>")
            {
                continue;
            }

            if (typeRefs[type] <= 1)
            {
                var location = type.Locations.FirstOrDefault();
                var lineSpan = location?.GetLineSpan();
                yield return new DiagnosticRecord(
                    FilePath: location?.SourceTree?.FilePath ?? string.Empty,
                    Plugin: "csharp-doctor",
                    Rule: RuleId,
                    Severity: DiagnosticSeverity.Info,
                    Message: $"Type '{type.ToDisplayString()}' appears unused in the solution",
                    Help: "Remove dead types or mark them with an explicit suppression if they are reached via reflection.",
                    Line: (lineSpan?.StartLinePosition.Line ?? 0) + 1,
                    Column: (lineSpan?.StartLinePosition.Character ?? 0) + 1,
                    Category: "Architecture",
                    Tags: ["solution-scoped", "architecture", "design"]);
            }
        }
    }

    private static void CollectTypes(INamespaceOrTypeSymbol symbol, List<INamedTypeSymbol> types)
    {
        if (symbol is INamedTypeSymbol named)
        {
            types.Add(named);
        }

        foreach (var member in symbol.GetMembers())
        {
            if (member is INamespaceOrTypeSymbol child)
            {
                CollectTypes(child, types);
            }
        }
    }
}
