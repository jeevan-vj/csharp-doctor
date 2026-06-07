using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpDoctor.Wiring.IntentDiscovery;

/// <summary>A concrete class that looks like an <c>IHostedService</c> / <c>BackgroundService</c>.</summary>
public sealed record HostedServiceCandidate(string TypeName, Location Location);

/// <summary>
/// Finds concrete hosted-service implementations by inspecting class base lists. Matching is
/// syntactic (by base-type identifier) so it works without a restored compilation.
/// </summary>
public static class HostedServiceFinder
{
    private static readonly HashSet<string> HostedServiceBaseNames = new(StringComparer.Ordinal)
    {
        "IHostedService",
        "BackgroundService",
        "IHostedLifecycleService",
    };

    public static IEnumerable<HostedServiceCandidate> Find(SyntaxNode root)
    {
        foreach (var classDeclaration in root.DescendantNodes().OfType<ClassDeclarationSyntax>())
        {
            if (classDeclaration.Modifiers.Any(SyntaxKind.AbstractKeyword))
            {
                continue;
            }

            if (classDeclaration.BaseList is null)
            {
                continue;
            }

            var matchesHostedService = classDeclaration.BaseList.Types
                .Select(static t => BaseTypeName(t.Type))
                .Any(name => name is not null && HostedServiceBaseNames.Contains(name));

            if (matchesHostedService)
            {
                yield return new HostedServiceCandidate(
                    classDeclaration.Identifier.ValueText,
                    classDeclaration.Identifier.GetLocation());
            }
        }
    }

    private static string? BaseTypeName(TypeSyntax type) => type switch
    {
        IdentifierNameSyntax identifier => identifier.Identifier.ValueText,
        QualifiedNameSyntax qualified => qualified.Right.Identifier.ValueText,
        GenericNameSyntax generic => generic.Identifier.ValueText,
        _ => null,
    };
}
