namespace CSharpDoctor.Core.Pipeline;

using CSharpDoctor.Core.Configuration;
using CSharpDoctor.Core.Diagnostics;

/// <summary>Filters diagnostics for a specific output surface.</summary>
public static class DiagnosticSurfaceFilter
{
    public static IReadOnlyList<DiagnosticRecord> FilterForSurface(
        IEnumerable<DiagnosticRecord> diagnostics,
        OutputSurface surface,
        CSharpDoctorConfig config)
    {
        var filter = surface switch
        {
            OutputSurface.Cli => config.Surfaces.Cli,
            OutputSurface.PrComment => config.Surfaces.PrComment,
            OutputSurface.Score => config.Surfaces.Score,
            OutputSurface.CiFailure => config.Surfaces.CiFailure,
            _ => config.Surfaces.Cli,
        };

        return diagnostics.Where(d => Matches(d, filter)).ToList();
    }

    private static bool Matches(DiagnosticRecord diagnostic, Configuration.SurfaceFilter filter)
    {
        if (filter.ExcludeTags.Any(tag => diagnostic.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase)))
        {
            return false;
        }

        if (filter.IncludeTags.Count == 0)
        {
            return true;
        }

        return filter.IncludeTags.Any(tag => diagnostic.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase));
    }
}
