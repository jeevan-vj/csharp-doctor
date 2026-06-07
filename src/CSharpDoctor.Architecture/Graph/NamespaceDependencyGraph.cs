using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpDoctor.Architecture.Graph;

public sealed class NamespaceDependencyGraph
{
    private readonly Dictionary<string, HashSet<string>> _edges = new(StringComparer.Ordinal);

    public IReadOnlyDictionary<string, HashSet<string>> Edges => _edges;

    public void AddEdge(string fromNamespace, string toNamespace)
    {
        if (string.IsNullOrEmpty(fromNamespace)
            || string.IsNullOrEmpty(toNamespace)
            || fromNamespace == toNamespace)
        {
            return;
        }

        if (!_edges.TryGetValue(fromNamespace, out var targets))
        {
            targets = new HashSet<string>(StringComparer.Ordinal);
            _edges[fromNamespace] = targets;
        }

        targets.Add(toNamespace);
        _edges.TryAdd(toNamespace, new HashSet<string>(StringComparer.Ordinal));
    }

    public static NamespaceDependencyGraph Build(Solution solution, CancellationToken cancellationToken)
    {
        var graph = new NamespaceDependencyGraph();

        foreach (var project in solution.Projects)
        {
            var compilation = project.GetCompilationAsync(cancellationToken).GetAwaiter().GetResult();
            if (compilation is null)
            {
                continue;
            }

            foreach (var document in project.Documents)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var model = document.GetSemanticModelAsync(cancellationToken).GetAwaiter().GetResult();
                var root = document.GetSyntaxRootAsync(cancellationToken).GetAwaiter().GetResult();
                if (model is null || root is null)
                {
                    continue;
                }

                var sourceNamespace = GetNamespace(model, root);
                if (sourceNamespace is null)
                {
                    continue;
                }

                foreach (var node in root.DescendantNodes())
                {
                    var symbol = model.GetSymbolInfo(node, cancellationToken).Symbol
                        ?? model.GetSymbolInfo(node, cancellationToken).CandidateSymbols.FirstOrDefault();
                    if (symbol is null)
                    {
                        continue;
                    }

                    var targetNamespace = GetSymbolNamespace(symbol);
                    if (targetNamespace is not null)
                    {
                        graph.AddEdge(sourceNamespace, targetNamespace);
                    }
                }
            }
        }

        return graph;
    }

    private static string? GetNamespace(SemanticModel model, SyntaxNode root)
    {
        var namespaceDecl = root.DescendantNodes()
            .FirstOrDefault(n => n is BaseNamespaceDeclarationSyntax);
        if (namespaceDecl is not null)
        {
            var symbol = model.GetDeclaredSymbol(namespaceDecl);
            var ns = symbol?.ToDisplayString();
            return string.IsNullOrEmpty(ns) ? null : ns;
        }

        var firstType = root.DescendantNodes().OfType<TypeDeclarationSyntax>().FirstOrDefault();
        if (firstType is not null)
        {
            var symbol = model.GetDeclaredSymbol(firstType);
            var ns = symbol?.ContainingNamespace?.ToDisplayString();
            return string.IsNullOrEmpty(ns) ? null : ns;
        }

        return null;
    }

    private static string? GetSymbolNamespace(ISymbol symbol)
    {
        var ns = symbol.ContainingNamespace?.ToDisplayString();
        return string.IsNullOrEmpty(ns) ? null : ns;
    }

    public static IReadOnlyList<IReadOnlyList<string>> FindCycles(NamespaceDependencyGraph graph)
    {
        var cycles = new List<IReadOnlyList<string>>();
        var visited = new HashSet<string>(StringComparer.Ordinal);
        var stack = new HashSet<string>(StringComparer.Ordinal);
        var path = new List<string>();

        foreach (var node in graph.Edges.Keys.OrderBy(static k => k, StringComparer.Ordinal))
        {
            if (!visited.Contains(node))
            {
                Dfs(node, graph, visited, stack, path, cycles);
            }
        }

        return cycles;
    }

    private static void Dfs(
        string node,
        NamespaceDependencyGraph graph,
        HashSet<string> visited,
        HashSet<string> stack,
        List<string> path,
        List<IReadOnlyList<string>> cycles)
    {
        visited.Add(node);
        stack.Add(node);
        path.Add(node);

        if (graph.Edges.TryGetValue(node, out var neighbors))
        {
            foreach (var neighbor in neighbors.OrderBy(static n => n, StringComparer.Ordinal))
            {
                if (!visited.Contains(neighbor))
                {
                    Dfs(neighbor, graph, visited, stack, path, cycles);
                }
                else if (stack.Contains(neighbor))
                {
                    var start = path.IndexOf(neighbor);
                    if (start >= 0)
                    {
                        cycles.Add(path.Skip(start).Append(neighbor).ToList());
                    }
                }
            }
        }

        stack.Remove(node);
        path.RemoveAt(path.Count - 1);
    }

    public static IReadOnlyList<(string First, string Second)> FindMutualDependencies(NamespaceDependencyGraph graph)
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
