using Microsoft.CodeAnalysis;

namespace CSharpDoctor.Architecture.Graph;

public sealed class SolutionReferenceIndex
{
    private readonly Dictionary<ISymbol, int> _referenceCounts = new(SymbolEqualityComparer.Default);

    public IReadOnlyDictionary<ISymbol, int> ReferenceCounts => _referenceCounts;

    public static SolutionReferenceIndex Build(Solution solution, CancellationToken cancellationToken)
    {
        var index = new SolutionReferenceIndex();
        var tracked = new HashSet<ISymbol>(SymbolEqualityComparer.Default);

        foreach (var project in solution.Projects)
        {
            var compilation = project.GetCompilationAsync(cancellationToken).GetAwaiter().GetResult();
            if (compilation is null)
            {
                continue;
            }

            foreach (var symbol in GetAllSymbols(compilation.Assembly.GlobalNamespace))
            {
                tracked.Add(symbol);
                index._referenceCounts[symbol] = 0;
            }
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
                    var symbol = model.GetSymbolInfo(node, cancellationToken).Symbol
                        ?? model.GetSymbolInfo(node, cancellationToken).CandidateSymbols.FirstOrDefault();
                    if (symbol is null)
                    {
                        continue;
                    }

                    var trackedSymbol = ResolveTracked(symbol, tracked);
                    if (trackedSymbol is not null)
                    {
                        index._referenceCounts[trackedSymbol]++;
                    }
                }
            }
        }

        return index;
    }

    private static ISymbol? ResolveTracked(ISymbol symbol, HashSet<ISymbol> tracked)
    {
        if (tracked.Contains(symbol))
        {
            return symbol;
        }

        return symbol switch
        {
            IMethodSymbol method when method.AssociatedSymbol is not null
                && tracked.Contains(method.AssociatedSymbol) => method.AssociatedSymbol,
            _ => symbol.ContainingType is not null && tracked.Contains(symbol.ContainingType)
                ? symbol.ContainingType
                : null,
        };
    }

    private static IEnumerable<ISymbol> GetAllSymbols(INamespaceOrTypeSymbol container)
    {
        foreach (var member in container.GetMembers())
        {
            switch (member)
            {
                case INamespaceSymbol ns:
                    foreach (var child in GetAllSymbols(ns))
                    {
                        yield return child;
                    }

                    break;
                case INamedTypeSymbol type:
                    yield return type;
                    foreach (var typeMember in type.GetMembers())
                    {
                        if (typeMember is IMethodSymbol { MethodKind: MethodKind.Ordinary or MethodKind.Constructor } method)
                        {
                            yield return method;
                        }
                        else if (typeMember is IFieldSymbol field)
                        {
                            yield return field;
                        }
                    }

                    foreach (var nested in type.GetTypeMembers())
                    {
                        foreach (var child in GetAllSymbols(nested))
                        {
                            yield return child;
                        }
                    }

                    break;
            }
        }
    }
}
