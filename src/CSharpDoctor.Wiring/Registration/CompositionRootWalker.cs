using Microsoft.CodeAnalysis;

namespace CSharpDoctor.Wiring.Registration;

/// <summary>
/// Collects the syntax roots where service registrations may live. Registration helpers can be
/// declared anywhere (composition root, extension methods, modules), so v0 returns every document
/// root in the solution rather than guessing at a single composition root.
/// </summary>
public static class CompositionRootWalker
{
    public static async Task<IReadOnlyList<SyntaxNode>> CollectRootsAsync(
        Solution solution,
        CancellationToken cancellationToken)
    {
        var roots = new List<SyntaxNode>();
        foreach (var project in solution.Projects)
        {
            foreach (var document in project.Documents)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false) is { } root)
                {
                    roots.Add(root);
                }
            }
        }

        return roots;
    }
}
