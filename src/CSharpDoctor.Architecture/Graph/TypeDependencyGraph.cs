using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpDoctor.Architecture.Graph;

public sealed class TypeDependencyGraph
{
    private readonly Dictionary<string, HashSet<string>> _edges = new(StringComparer.Ordinal);

    public IReadOnlyDictionary<string, HashSet<string>> Edges => _edges;

    public void AddEdge(string fromType, string toType)
    {
        if (string.IsNullOrEmpty(fromType)
            || string.IsNullOrEmpty(toType)
            || fromType == toType)
        {
            return;
        }

        if (!_edges.TryGetValue(fromType, out var targets))
        {
            targets = new HashSet<string>(StringComparer.Ordinal);
            _edges[fromType] = targets;
        }

        targets.Add(toType);
        _edges.TryAdd(toType, new HashSet<string>(StringComparer.Ordinal));
    }

    public static TypeDependencyGraph Build(Solution solution, CancellationToken cancellationToken)
    {
        var graph = new TypeDependencyGraph();

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

                var containingType = root.DescendantNodes().OfType<TypeDeclarationSyntax>().FirstOrDefault();
                if (containingType is null)
                {
                    continue;
                }

                var sourceSymbol = model.GetDeclaredSymbol(containingType);
                var sourceName = sourceSymbol?.ToDisplayString();
                if (sourceName is null)
                {
                    continue;
                }

                foreach (var node in root.DescendantNodes())
                {
                    var symbol = model.GetSymbolInfo(node, cancellationToken).Symbol;
                    if (symbol?.ContainingType is INamedTypeSymbol targetType)
                    {
                        graph.AddEdge(sourceName, targetType.ToDisplayString());
                    }
                }
            }
        }

        return graph;
    }

    public static IReadOnlyList<(string First, string Second)> FindMutualDependencies(TypeDependencyGraph graph)
    {
        var pairs = new List<(string, string)>();

        foreach (var (from, targets) in graph.Edges)
        {
            foreach (var target in targets)
            {
                if (graph.Edges.TryGetValue(target, out var reverse)
                    && reverse.Contains(from)
                    && string.CompareOrdinal(from, target) < 0)
                {
                    pairs.Add((from, target));
                }
            }
        }

        return pairs;
    }
}
