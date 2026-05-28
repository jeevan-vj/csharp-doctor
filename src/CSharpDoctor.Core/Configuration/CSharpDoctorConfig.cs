using System.Text.Json.Serialization;

namespace CSharpDoctor.Core.Configuration;

/// <summary>Resolved CSharp Doctor configuration.</summary>
public sealed record CSharpDoctorConfig
{
    public bool Lint { get; init; } = true;

    public bool DeadCode { get; init; } = true;

    public string? RootDir { get; init; }

    public FailOnLevel FailOn { get; init; } = FailOnLevel.Error;

    public IReadOnlyDictionary<string, SeverityOverride> Rules { get; init; } =
        new Dictionary<string, SeverityOverride>(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyDictionary<string, SeverityOverride> Categories { get; init; } =
        new Dictionary<string, SeverityOverride>(StringComparer.OrdinalIgnoreCase);

    public IgnoreConfig Ignore { get; init; } = new();

    public SurfaceConfig Surfaces { get; init; } = SurfaceConfig.CreateDefault();

    public bool RespectInlineDisables { get; init; } = true;

    [JsonIgnore]
    public string? ConfigFileDirectory { get; init; }
}

public enum FailOnLevel
{
    Error,
    Warning,
    None,
}

public sealed class IgnoreConfig
{
    public IReadOnlyList<string> Rules { get; init; } = [];

    public IReadOnlyList<string> Files { get; init; } = [];

    public IReadOnlyList<string> Tags { get; init; } = [];
}

public sealed class SurfaceConfig
{
    public SurfaceFilter Cli { get; init; } = new();

    public SurfaceFilter PrComment { get; init; } = new() { ExcludeTags = ["design"] };

    public SurfaceFilter Score { get; init; } = new() { ExcludeTags = ["design"] };

    public SurfaceFilter CiFailure { get; init; } = new() { ExcludeTags = ["design"] };

    public static SurfaceConfig CreateDefault() => new();
}

public sealed class SurfaceFilter
{
    public IReadOnlyList<string> IncludeTags { get; init; } = [];

    public IReadOnlyList<string> ExcludeTags { get; init; } = [];
}
