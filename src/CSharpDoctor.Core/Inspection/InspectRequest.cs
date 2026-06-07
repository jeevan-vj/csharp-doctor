using CSharpDoctor.Core.Configuration;

namespace CSharpDoctor.Core.Inspection;

/// <summary>Input for a single inspection run.</summary>
public sealed record InspectRequest(
    string TargetDirectory,
    InspectMode Mode = InspectMode.Full,
    string? DiffBase = null,
    IReadOnlyList<string>? IncludeFiles = null,
    CliConfigOverrides? CliOverrides = null,
    OutputFormat OutputFormat = OutputFormat.Console,
    bool GitHubAnnotations = false);

public enum OutputFormat
{
    Console,
    Json,
    Sarif,
    Html,
}
