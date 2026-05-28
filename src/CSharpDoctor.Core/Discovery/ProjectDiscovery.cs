namespace CSharpDoctor.Core.Discovery;

/// <summary>Discovers solutions and projects under a directory.</summary>
public static class ProjectDiscovery
{
    public static IReadOnlyList<string> FindProjects(string directory)
    {
        var root = Path.GetFullPath(directory);
        if (!Directory.Exists(root))
        {
            return [];
        }

        var solutions = Directory.GetFiles(root, "*.sln", SearchOption.AllDirectories);
        if (solutions.Length > 0)
        {
            return solutions.OrderBy(static p => p, StringComparer.OrdinalIgnoreCase).ToList();
        }

        return FindCsprojFiles(root);
    }

    public static IReadOnlyList<string> FindCsprojFiles(string directory)
    {
        var root = Path.GetFullPath(directory);
        if (!Directory.Exists(root))
        {
            return [];
        }

        return Directory.GetFiles(root, "*.csproj", SearchOption.AllDirectories)
            .Where(p => !p.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .OrderBy(static p => p, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public static IReadOnlyList<string> FindSourceFiles(string projectOrDirectory, IReadOnlyList<string>? includeOnly = null)
    {
        var directory = File.Exists(projectOrDirectory)
            ? Path.GetDirectoryName(projectOrDirectory) ?? projectOrDirectory
            : projectOrDirectory;

        var files = Directory.GetFiles(directory, "*.cs", SearchOption.AllDirectories)
            .Where(p => !p.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .Where(p => !p.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal));

        if (includeOnly is { Count: > 0 })
        {
            var normalized = includeOnly.Select(Normalize).ToHashSet(StringComparer.OrdinalIgnoreCase);
            files = files.Where(f => normalized.Contains(Normalize(f)));
        }

        return files.ToList();
    }

    private static string Normalize(string path) =>
        Path.GetFullPath(path).Replace('\\', '/');
}
