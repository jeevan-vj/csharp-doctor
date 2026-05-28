namespace CSharpDoctor.Core.Pipeline;

using CSharpDoctor.Core.Configuration;
using CSharpDoctor.Core.Diagnostics;

/// <summary>Filters diagnostics by ignored rules, files, and tags.</summary>
public sealed class IgnoreTransform : IDiagnosticTransform
{
    private readonly IgnoreConfig _ignore;

    public IgnoreTransform(CSharpDoctorConfig config) => _ignore = config.Ignore;

    public DiagnosticRecord? Apply(DiagnosticRecord diagnostic)
    {
        var ruleKey = $"{diagnostic.Plugin}/{diagnostic.Rule}";
        if (_ignore.Rules.Contains(ruleKey, StringComparer.OrdinalIgnoreCase)
            || _ignore.Rules.Contains(diagnostic.Rule, StringComparer.OrdinalIgnoreCase))
        {
            return null;
        }

        if (_ignore.Tags.Any(tag => diagnostic.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase)))
        {
            return null;
        }

        foreach (var pattern in _ignore.Files)
        {
            if (MatchesGlob(diagnostic.FilePath, pattern))
            {
                return null;
            }
        }

        return diagnostic;
    }

    private static bool MatchesGlob(string filePath, string pattern)
    {
        if (pattern.Contains('*', StringComparison.Ordinal))
        {
            var regex = "^" + System.Text.RegularExpressions.Regex.Escape(pattern)
                .Replace("\\*", ".*", StringComparison.Ordinal) + "$";
            return System.Text.RegularExpressions.Regex.IsMatch(
                filePath.Replace('\\', '/'),
                regex,
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }

        return filePath.Contains(pattern, StringComparison.OrdinalIgnoreCase);
    }
}
