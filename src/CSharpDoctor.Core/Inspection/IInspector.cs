using CSharpDoctor.Core.Configuration;
using CSharpDoctor.Core.Diagnostics;
using CSharpDoctor.Core.Discovery;
using Microsoft.CodeAnalysis;

namespace CSharpDoctor.Core.Inspection;

/// <summary>A single analysis pass (lint, wiring, ef, ...) that produces diagnostics.</summary>
public interface IInspector
{
    /// <summary>Stable identifier used for config gating and reporting (e.g. "lint", "wiring", "ef").</summary>
    string Name { get; }

    Task<IReadOnlyList<DiagnosticRecord>> InspectAsync(
        InspectContext context,
        CancellationToken cancellationToken = default);
}

/// <summary>Shared inputs handed to every inspector for a single run.</summary>
public sealed record InspectContext(
    InspectRequest Request,
    CSharpDoctorConfig Config,
    IReadOnlyList<string> SourceFiles,
    IReadOnlyList<string> CsprojPaths,
    string ScanRoot,
    IReadOnlyDictionary<string, ProjectCapabilities> Capabilities,
    Lazy<Task<Solution?>> SolutionLazy);
