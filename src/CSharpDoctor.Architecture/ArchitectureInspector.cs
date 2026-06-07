using CSharpDoctor.Architecture.Graph;
using CSharpDoctor.Architecture.Rules;
using CSharpDoctor.Core.Diagnostics;
using CSharpDoctor.Core.Inspection;

namespace CSharpDoctor.Architecture;

/// <summary>Whole-solution inspector for namespace dependency and dead-code heuristics.</summary>
public sealed class ArchitectureInspector : IInspector
{
    public string Name => InspectorRegistry.ArchitectureInspectorName;

    public async Task<IReadOnlyList<DiagnosticRecord>> InspectAsync(
        InspectContext context,
        CancellationToken cancellationToken = default)
    {
        var solution = await context.SolutionLazy.Value.ConfigureAwait(false);
        if (solution is null)
        {
            return [];
        }

        var graph = NamespaceDependencyGraph.Build(solution, cancellationToken);
        var typeGraph = TypeDependencyGraph.Build(solution, cancellationToken);
        var referenceIndex = SolutionReferenceIndex.Build(solution, cancellationToken);

        var diagnostics = new List<DiagnosticRecord>();
        diagnostics.AddRange(Csda0001NamespaceDependencyCycle.Run(graph));
        diagnostics.AddRange(Csda0002MutualNamespaceDependency.Run(graph));
        diagnostics.AddRange(Csda0003PotentiallyDeadType.Run(solution, cancellationToken));
        diagnostics.AddRange(Csda0004ExcessiveClassCoupling.Run(solution, cancellationToken));
        diagnostics.AddRange(Csda0005DuplicateTypeAcrossAssemblies.Run(solution, cancellationToken));
        diagnostics.AddRange(Csda0006PotentiallyDeadMethod.Run(referenceIndex, cancellationToken));
        diagnostics.AddRange(Csda0007PotentiallyDeadField.Run(referenceIndex, cancellationToken));
        diagnostics.AddRange(Csda0008MutuallyDependentTypes.Run(typeGraph));
        diagnostics.AddRange(Csda0009SourceFileOutsideProject.Run(context.CsprojPaths, cancellationToken));
        diagnostics.AddRange(Csda0010ForbiddenNamespaceDependency.Run(graph, context.Config.ArchitecturePolicy));
        diagnostics.AddRange(Csda0011ForbiddenAssemblyReference.Run(solution, context.Config.ArchitecturePolicy));
        return diagnostics;
    }
}
