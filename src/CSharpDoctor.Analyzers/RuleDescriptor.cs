using Microsoft.CodeAnalysis;

namespace CSharpDoctor.Analyzers;

/// <summary>Metadata for a CSharp Doctor analyzer rule.</summary>
public sealed class RuleDescriptor
{
    public RuleDescriptor(
        string id,
        string title,
        string category,
        DiagnosticSeverity defaultSeverity,
        string messageFormat,
        string description,
        IReadOnlyList<string> tags,
        bool enabledByDefault = true)
    {
        Id = id;
        Title = title;
        Category = category;
        DefaultSeverity = defaultSeverity;
        MessageFormat = messageFormat;
        Description = description;
        Tags = tags;
        EnabledByDefault = enabledByDefault;
    }

    public string Id { get; }

    public string Title { get; }

    public string Category { get; }

    public DiagnosticSeverity DefaultSeverity { get; }

    public string MessageFormat { get; }

    public string Description { get; }

    public IReadOnlyList<string> Tags { get; }

    public bool EnabledByDefault { get; }

    public DiagnosticDescriptor ToDescriptor() =>
        new(
            Id,
            Title,
            MessageFormat,
            Category,
            DefaultSeverity,
            isEnabledByDefault: EnabledByDefault,
            description: Description,
            customTags: Tags.ToArray());
}
