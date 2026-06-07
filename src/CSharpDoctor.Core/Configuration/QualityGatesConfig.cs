using System.Text.Json.Serialization;

namespace CSharpDoctor.Core.Configuration;

/// <summary>Aggregate thresholds that can fail CI independently of per-rule severity.</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record QualityGatesConfig
{
    public bool Enabled { get; init; }

    public int? MaxErrors { get; init; }

    public int? MaxWarnings { get; init; }

    public int? MaxTotalIssues { get; init; }

    public int? MaxDebtScore { get; init; }

    public int? MaxCriticalIssues { get; init; }

    public int? MaxNewIssues { get; init; }

    public string? BaselinePath { get; init; }

    public IReadOnlyDictionary<string, int> MaxIssuesPerRule { get; init; } =
        new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

    public DebtWeightsConfig DebtWeights { get; init; } = new();
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record DebtWeightsConfig
{
    public int Error { get; init; } = 10;

    public int Warning { get; init; } = 3;

    public int Info { get; init; } = 1;
}

/// <summary>Limits for hot-spot prioritization views in JSON/console output.</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record HotSpotsConfig
{
    public int TopRules { get; init; } = 10;

    public int TopFiles { get; init; } = 10;

    public int TopElements { get; init; } = 15;
}
