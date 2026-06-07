using CSharpDoctor.Core.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using DiagnosticSeverity = CSharpDoctor.Core.Diagnostics.DiagnosticSeverity;

namespace CSharpDoctor.Architecture.Rules;

public static class Csda0004ExcessiveClassCoupling
{
    public const string RuleId = "CSDA0004";
    private const int MaxCoupledTypes = 20;

    public static IEnumerable<DiagnosticRecord> Run(Solution solution, CancellationToken cancellationToken)
    {
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

                foreach (var typeDecl in root.DescendantNodes().OfType<TypeDeclarationSyntax>())
                {
                    var typeSymbol = model.GetDeclaredSymbol(typeDecl) as INamedTypeSymbol;
                    if (typeSymbol is null)
                    {
                        continue;
                    }

                    var coupled = new HashSet<string>(StringComparer.Ordinal);
                    foreach (var node in typeDecl.DescendantNodes())
                    {
                        var symbol = model.GetSymbolInfo(node, cancellationToken).Symbol;
                        if (symbol?.ContainingType is INamedTypeSymbol other
                            && !SymbolEqualityComparer.Default.Equals(other, typeSymbol))
                        {
                            coupled.Add(other.ToDisplayString());
                        }
                    }

                    if (coupled.Count > MaxCoupledTypes)
                    {
                        var location = typeSymbol.Locations.FirstOrDefault();
                        var lineSpan = location?.GetLineSpan();
                        yield return new DiagnosticRecord(
                            FilePath: location?.SourceTree?.FilePath ?? string.Empty,
                            Plugin: "csharp-doctor",
                            Rule: RuleId,
                            Severity: DiagnosticSeverity.Info,
                            Message: $"Type '{typeSymbol.Name}' is coupled to {coupled.Count} other types",
                            Help: "Reduce coupling by introducing abstractions or splitting responsibilities.",
                            Line: (lineSpan?.StartLinePosition.Line ?? 0) + 1,
                            Column: (lineSpan?.StartLinePosition.Character ?? 0) + 1,
                            Category: "Architecture",
                            Tags: ["solution-scoped", "architecture", "design"]);
                    }
                }
            }
        }
    }
}
