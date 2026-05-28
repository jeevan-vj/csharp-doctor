namespace CSharpDoctor.Core.Pipeline;

using CSharpDoctor.Core.Configuration;
using CSharpDoctor.Core.Discovery;

/// <summary>Creates the default post-analysis pipeline.</summary>
public static class DiagnosticPipelineFactory
{
    public static DiagnosticPipeline CreateDefault(
        CSharpDoctorConfig config,
        Func<string, IReadOnlyList<string>> readFileLines,
        IReadOnlyDictionary<string, ProjectCapabilities>? fileCapabilities = null)
    {
        var transforms = new List<IDiagnosticTransform>
        {
            new SeverityOverrideTransform(config),
            new IgnoreTransform(config),
            new InlineSuppressionTransform(config, readFileLines),
        };

        if (fileCapabilities is { Count: > 0 })
        {
            transforms.Add(new CapabilityFilterTransform(fileCapabilities));
        }

        return new(transforms);
    }
}
