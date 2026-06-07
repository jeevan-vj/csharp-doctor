using CSharpDoctor.Core.Configuration;

namespace CSharpDoctor.Core.Inspection;

/// <summary>Resolves which inspectors run for a given configuration.</summary>
public static class InspectorRegistry
{
    public const string WiringInspectorName = "wiring";

    public const string EfInspectorName = "ef";

    public const string ArchitectureInspectorName = "architecture";

    public const string ScaInspectorName = "sca";

    /// <summary>
    /// Builds the ordered list of inspectors enabled by <paramref name="config"/>.
    /// External inspectors (wiring, ef) live in separate assemblies and are supplied via
    /// <paramref name="externalFactories"/>; they only run when both enabled in config and a factory exists.
    /// </summary>
    public static IReadOnlyList<IInspector> Resolve(
        CSharpDoctorConfig config,
        Func<IInspector> lintFactory,
        IReadOnlyDictionary<string, Func<IInspector>>? externalFactories = null)
    {
        var inspectors = new List<IInspector>();

        if (config.Checks.Lint)
        {
            inspectors.Add(lintFactory());
        }

        if (config.Checks.Wiring && TryGet(externalFactories, WiringInspectorName, out var wiring))
        {
            inspectors.Add(wiring());
        }

        if (config.Checks.Ef && TryGet(externalFactories, EfInspectorName, out var ef))
        {
            inspectors.Add(ef());
        }

        if (config.Checks.Architecture && TryGet(externalFactories, ArchitectureInspectorName, out var architecture))
        {
            inspectors.Add(architecture());
        }

        if (config.Checks.Sca && TryGet(externalFactories, ScaInspectorName, out var sca))
        {
            inspectors.Add(sca());
        }

        return inspectors;
    }

    private static bool TryGet(
        IReadOnlyDictionary<string, Func<IInspector>>? factories,
        string key,
        out Func<IInspector> factory)
    {
        if (factories is not null && factories.TryGetValue(key, out var found))
        {
            factory = found;
            return true;
        }

        factory = null!;
        return false;
    }
}
