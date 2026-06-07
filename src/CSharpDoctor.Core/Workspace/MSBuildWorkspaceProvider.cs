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

        if (projects[0].EndsWith(".sln", StringComparison.OrdinalIgnoreCase))
        {
            await workspace.OpenSolutionAsync(projects[0], cancellationToken: cancellationToken).ConfigureAwait(false);
        }
        else
        {
            foreach (var project in projects)
            {
                await workspace.OpenProjectAsync(project, cancellationToken: cancellationToken).ConfigureAwait(false);
            }
        }

        return workspace.CurrentSolution;
    }

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
