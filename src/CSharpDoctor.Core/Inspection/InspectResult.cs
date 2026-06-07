using CSharpDoctor.Core.Diagnostics;
using CSharpDoctor.Core.Reporting;

namespace CSharpDoctor.Core.Inspection;

/// <summary>Result of a completed inspection.</summary>
public sealed record InspectResult(
    IReadOnlyList<DiagnosticRecord> Diagnostics,
    int ErrorCount,
    int WarningCount,
    TimeSpan Elapsed,
    int ExitCode,
    IReadOnlyDictionary<string, InspectorRunInfo>? InspectorRuns = null,
    ScanMetrics? Metrics = null,
    QualityGateEvaluation? QualityGates = null,
    HotSpotsReport? HotSpots = null);

/// <summary>Per-inspector timing and counts for a single run.</summary>
public sealed record InspectorRunInfo(TimeSpan Elapsed, int DiagnosticCount, bool Ran);
