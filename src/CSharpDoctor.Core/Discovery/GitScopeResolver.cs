using System.Diagnostics;
using CSharpDoctor.Core.Inspection;

namespace CSharpDoctor.Core.Discovery;

/// <summary>Resolves changed or staged files via git.</summary>
public static class GitScopeResolver
{
    private static readonly string[] SourceExtensions = [".cs", ".csproj", ".razor"];

    public static IReadOnlyList<string> ResolveIncludePaths(string directory, InspectMode mode, string? diffBase)
    {
        var args = mode switch
        {
            InspectMode.Staged => "diff --cached --name-only --diff-filter=ACMR",
            InspectMode.Diff => $"diff --name-only --diff-filter=ACMR {diffBase ?? "HEAD"}...HEAD",
            _ => null,
        };

        if (args is null)
        {
            return [];
        }

        var output = RunGit(directory, args);
        return output
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(line => SourceExtensions.Any(ext => line.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
            .Select(line => Path.GetFullPath(Path.Combine(directory, line)))
            .ToList();
    }

    private static string RunGit(string directory, string arguments)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = arguments,
                WorkingDirectory = directory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
            },
        };

        process.Start();
        var stdout = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        return stdout;
    }
}
