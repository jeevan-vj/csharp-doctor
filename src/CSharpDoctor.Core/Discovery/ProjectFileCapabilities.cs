namespace CSharpDoctor.Core.Discovery;

/// <summary>Maps source files to project capabilities using the nearest containing .csproj.</summary>
public static class ProjectFileCapabilities
{
    public static IReadOnlyDictionary<string, ProjectCapabilities> Build(
        IReadOnlyList<string> projectPaths,
        IReadOnlyList<string> sourceFiles)
    {
        var projectCaps = projectPaths.ToDictionary(
            static p => Path.GetFullPath(p),
            ProjectCapabilities.FromCsproj,
            StringComparer.OrdinalIgnoreCase);

        var result = new Dictionary<string, ProjectCapabilities>(StringComparer.OrdinalIgnoreCase);
        var roots = projectPaths
            .Select(static p => (Project: Path.GetFullPath(p), Dir: Path.GetDirectoryName(Path.GetFullPath(p))!))
            .OrderByDescending(static x => x.Dir.Length)
            .ToList();

        foreach (var file in sourceFiles)
        {
            var full = Path.GetFullPath(file);
            var match = roots.FirstOrDefault(x => full.StartsWith(x.Dir + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
                || full.StartsWith(x.Dir + '/', StringComparison.OrdinalIgnoreCase));
            result[full] = match.Project is not null && projectCaps.TryGetValue(match.Project, out var caps)
                ? caps
                : ProjectCapabilities.None;
        }

        return result;
    }
}
