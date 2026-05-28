using CSharpDoctor.Core.Diagnostics;

namespace CSharpDoctor.Core.Inspection;

/// <summary>Result of a completed inspection.</summary>
public sealed record InspectResult(
    IReadOnlyList<DiagnosticRecord> Diagnostics,
    int ErrorCount,
    int WarningCount,
    TimeSpan Elapsed,
    int ExitCode);
