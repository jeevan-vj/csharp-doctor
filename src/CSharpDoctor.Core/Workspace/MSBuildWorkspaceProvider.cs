using CSharpDoctor.Core.Discovery;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

namespace CSharpDoctor.Core.Workspace;

/// <summary>
/// Loads a Roslyn <see cref="Solution"/> via MSBuild, registering the installed SDK with
/// <see cref="MSBuildLocator"/> on first use. Results are cached per target path and the underlying
/// workspaces are kept alive for the lifetime of the provider (the loaded <see cref="Solution"/>
/// snapshots reference them).
/// </summary>
public sealed class MSBuildWorkspaceProvider : IDisposable
{
    private static readonly object RegistrationGate = new();

    private readonly object _gate = new();
    private readonly Dictionary<string, Task<Solution?>> _cache = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<MSBuildWorkspace> _workspaces = [];
    private readonly TextWriter? _diagnostics;

    public MSBuildWorkspaceProvider(TextWriter? diagnostics = null) => _diagnostics = diagnostics;

    public Task<Solution?> LoadAsync(string targetDirectory, CancellationToken cancellationToken = default)
    {
        var key = Path.GetFullPath(targetDirectory);
        lock (_gate)
        {
            if (_cache.TryGetValue(key, out var existing))
            {
                return existing;
            }

            var task = LoadCoreAsync(key, cancellationToken);
            _cache[key] = task;
            return task;
        }
    }

    private async Task<Solution?> LoadCoreAsync(string directory, CancellationToken cancellationToken)
    {
        EnsureLocatorRegistered();

        var projects = ProjectDiscovery.FindProjects(directory);
        if (projects.Count == 0)
        {
            _diagnostics?.WriteLine($"No solution or project found under '{directory}'.");
            return null;
        }

        var workspace = MSBuildWorkspace.Create();
        lock (_gate)
        {
            _workspaces.Add(workspace);
        }

        workspace.WorkspaceFailed += (_, e) => _diagnostics?.WriteLine($"workspace: {e.Diagnostic.Message}");

        // Roslyn 4.12 OpenSolutionAsync reads classic .sln files only. .slnx is XML; open the
        // projects it lists. OpenProjectAsync also loads ProjectReferences, so a later explicit
        // open of a project already pulled in throws "already part of the workspace".
        if (ProjectDiscovery.IsSolutionFile(projects[0]))
        {
            if (projects[0].EndsWith(".slnx", StringComparison.OrdinalIgnoreCase))
            {
                foreach (var project in ProjectDiscovery.ReadSlnxProjectPaths(projects[0]))
                {
                    await OpenProjectIfNeededAsync(workspace, project, cancellationToken).ConfigureAwait(false);
                }
            }
            else
            {
                await workspace.OpenSolutionAsync(projects[0], cancellationToken: cancellationToken).ConfigureAwait(false);
            }
        }
        else
        {
            foreach (var project in projects)
            {
                await OpenProjectIfNeededAsync(workspace, project, cancellationToken).ConfigureAwait(false);
            }
        }

        return workspace.CurrentSolution;
    }

    private async Task OpenProjectIfNeededAsync(
        MSBuildWorkspace workspace,
        string projectPath,
        CancellationToken cancellationToken)
    {
        var fullPath = Path.GetFullPath(projectPath);
        if (IsProjectLoaded(workspace.CurrentSolution, fullPath))
        {
            return;
        }

        try
        {
            await workspace.OpenProjectAsync(fullPath, cancellationToken: cancellationToken).ConfigureAwait(false);
        }
        catch (ArgumentException ex) when (ex.Message.Contains("already part of the workspace", StringComparison.Ordinal))
        {
            _diagnostics?.WriteLine($"workspace: skipped '{fullPath}' ({ex.Message})");
        }
    }

    private static bool IsProjectLoaded(Solution solution, string fullPath) =>
        solution.Projects.Any(project =>
            project.FilePath is not null
            && string.Equals(Path.GetFullPath(project.FilePath), fullPath, StringComparison.OrdinalIgnoreCase));

    private static void EnsureLocatorRegistered()
    {
        if (MSBuildLocator.IsRegistered)
        {
            return;
        }

        lock (RegistrationGate)
        {
            if (!MSBuildLocator.IsRegistered)
            {
                MSBuildLocator.RegisterDefaults();
            }
        }
    }

    public void Dispose()
    {
        lock (_gate)
        {
            foreach (var workspace in _workspaces)
            {
                workspace.Dispose();
            }

            _workspaces.Clear();
            _cache.Clear();
        }
    }
}
