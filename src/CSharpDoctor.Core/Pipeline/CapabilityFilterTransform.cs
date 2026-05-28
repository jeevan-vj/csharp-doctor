namespace CSharpDoctor.Core.Pipeline;

using CSharpDoctor.Core.Diagnostics;
using CSharpDoctor.Core.Discovery;

/// <summary>Suppresses diagnostics when required project capabilities are missing.</summary>
public sealed class CapabilityFilterTransform : IDiagnosticTransform
{
    private static readonly Dictionary<string, string> TagCapabilityRequirements =
        new(StringComparer.OrdinalIgnoreCase) { ["aspnet"] = "aspnet" };

    private readonly IReadOnlyDictionary<string, ProjectCapabilities> _fileCapabilities;

    public CapabilityFilterTransform(IReadOnlyDictionary<string, ProjectCapabilities> fileCapabilities) =>
        _fileCapabilities = fileCapabilities;

    public DiagnosticRecord? Apply(DiagnosticRecord diagnostic)
    {
        var path = Path.GetFullPath(diagnostic.FilePath);
        _fileCapabilities.TryGetValue(path, out var capabilities);
        capabilities ??= ProjectCapabilities.None;

        foreach (var tag in diagnostic.Tags)
        {
            if (TagCapabilityRequirements.TryGetValue(tag, out var required)
                && !capabilities.Has(required))
            {
                return null;
            }
        }

        return diagnostic;
    }
}
