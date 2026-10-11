using System.Xml.Linq;

namespace CSharpDoctor.Core.Discovery;

/// <summary>Discovers solutions and projects under a directory.</summary>
public static class ProjectDiscovery
{
    public static bool IsSolutionFile(string path) =>
        path.EndsWith(".sln", StringComparison.OrdinalIgnoreCase)
        || path.EndsWith(".slnx", StringComparison.OrdinalIgnoreCase);

    public static IReadOnlyList<string> FindProjects(string directory)
    {
        var root = Path.GetFullPath(directory);
        if (!Directory.Exists(root))
        {
            return [];
        }

        var solutions = Directory.GetFiles(root, "*.sln", SearchOption.AllDirectories)
            .Concat(Directory.GetFiles(root, "*.slnx", SearchOption.AllDirectories))
            .Where(static path => IsSolutionFile(path))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(static path => path.Count(static c => c is '\\' or '/'))
            .ThenBy(static path => path, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (solutions.Count > 0)
        {
            return solutions;
        }

        return FindCsprojFiles(root);
    }

    /// <summary>
    /// Directory whose <c>*.cs</c> files are in scope. A directory target is scanned as given.
    /// A file target is scanned from its parent directory.
    /// </summary>
    public static string ResolveScanRoot(string target)
    {
        var full = Path.GetFullPath(target);
        if (File.Exists(full))
        {
            return Path.GetDirectoryName(full) ?? full;
        }

        return full;
    }

    /// <summary>Project file paths declared by an XML solution (<c>.slnx</c>), resolved against the solution directory.</summary>
    public static IReadOnlyList<string> ReadSlnxProjectPaths(string slnxPath)
    {
        var fullSlnx = Path.GetFullPath(slnxPath);
        var baseDir = Path.GetDirectoryName(fullSlnx) ?? Directory.GetCurrentDirectory();
        var document = XDocument.Load(fullSlnx);

        return document.Descendants()
            .Where(static element => element.Name.LocalName == "Project")
            .Select(element => element.Attribute("Path")?.Value)
            .Where(static path => !string.IsNullOrWhiteSpace(path))
            .Select(path => Path.GetFullPath(Path.Combine(baseDir, path!)))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
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
