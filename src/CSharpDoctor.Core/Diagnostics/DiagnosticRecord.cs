namespace CSharpDoctor.Core.Diagnostics;

/// <summary>Normalized finding emitted by analyzers and the CLI pipeline.</summary>
public sealed record DiagnosticRecord(
    string FilePath,
    string Plugin,
    string Rule,
    DiagnosticSeverity Severity,
    string Message,
    string Help,
    int Line,
    int Column,
    string Category,
    IReadOnlyList<string> Tags,
    string? Url = null,
    string? SuppressionHint = null);
