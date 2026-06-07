using CSharpDoctor.Core.Diagnostics;
using CSharpDoctor.Core.Inspection;
using CSharpDoctor.Wiring.IntentDiscovery;
using CSharpDoctor.Wiring.Registration;
using CSharpDoctor.Wiring.Rules;

namespace CSharpDoctor.Wiring;

/// <summary>Whole-solution inspector that detects declared-but-unwired components.</summary>
public sealed class WiringInspector : IInspector
{
    public string Name => InspectorRegistry.WiringInspectorName;

    public async Task<IReadOnlyList<DiagnosticRecord>> InspectAsync(
        InspectContext context,
        CancellationToken cancellationToken = default)
    {
        var solution = await context.SolutionLazy.Value.ConfigureAwait(false);
        if (solution is null)
        {
            return [];
        }

        var roots = await CompositionRootWalker.CollectRootsAsync(solution, cancellationToken).ConfigureAwait(false);

        var hostedCandidates = roots.SelectMany(HostedServiceFinder.Find).ToList();
        var registrations = RegistrationGraph.Build(roots);
        var dependencies = DependencyFinder.Find(roots, registrations).ToList();

        var diagnostics = new List<DiagnosticRecord>();
        diagnostics.AddRange(Csdw0001UnregisteredHostedService.Run(hostedCandidates, registrations));
        diagnostics.AddRange(Csdw0008UnresolvedDependency.Run(dependencies, registrations));
        diagnostics.AddRange(Csdw0010CaptiveDependency.Run(dependencies, registrations));

        return diagnostics;
    }
}
