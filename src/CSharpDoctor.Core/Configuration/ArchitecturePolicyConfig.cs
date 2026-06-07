using System.Text.Json.Serialization;

namespace CSharpDoctor.Core.Configuration;

/// <summary>Config-driven architecture dependency policies (NDepend-style custom rules).</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record ArchitecturePolicyConfig
{
    public IReadOnlyList<ForbiddenDependencyRule> ForbiddenNamespaceDependencies { get; init; } = [];

    public IReadOnlyList<ForbiddenDependencyRule> ForbiddenAssemblyReferences { get; init; } = [];
}

/// <summary>A forbidden dependency edge. Use <c>*</c> suffix for prefix matching.</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record ForbiddenDependencyRule(string From, string To);
