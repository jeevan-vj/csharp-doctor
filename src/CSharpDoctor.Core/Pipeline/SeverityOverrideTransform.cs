namespace CSharpDoctor.Core.Pipeline;

using CSharpDoctor.Core.Configuration;
using CSharpDoctor.Core.Diagnostics;

/// <summary>Applies rule and category severity overrides; drops rules set to Off.</summary>
public sealed class SeverityOverrideTransform : IDiagnosticTransform
{
    private readonly IReadOnlyDictionary<string, SeverityOverride> _rules;
    private readonly IReadOnlyDictionary<string, SeverityOverride> _categories;

    public SeverityOverrideTransform(CSharpDoctorConfig config)
    {
        _rules = config.Rules;
        _categories = config.Categories;
    }

    public DiagnosticRecord? Apply(DiagnosticRecord diagnostic)
    {
        var ruleKey = $"{diagnostic.Plugin}/{diagnostic.Rule}";

        if (_rules.TryGetValue(ruleKey, out var ruleOverride))
        {
            return ApplyOverride(diagnostic, ruleOverride);
        }

        if (_rules.TryGetValue(diagnostic.Rule, out ruleOverride))
        {
            return ApplyOverride(diagnostic, ruleOverride);
        }

        if (_categories.TryGetValue(diagnostic.Category, out var categoryOverride))
        {
            return ApplyOverride(diagnostic, categoryOverride);
        }

        return diagnostic;
    }

    private static DiagnosticRecord? ApplyOverride(DiagnosticRecord diagnostic, SeverityOverride severityOverride) =>
        severityOverride switch
        {
            SeverityOverride.Off => null,
            SeverityOverride.Error => diagnostic with { Severity = DiagnosticSeverity.Error },
            SeverityOverride.Warning => diagnostic with { Severity = DiagnosticSeverity.Warning },
            SeverityOverride.Info => diagnostic with { Severity = DiagnosticSeverity.Info },
            _ => diagnostic,
        };
}
