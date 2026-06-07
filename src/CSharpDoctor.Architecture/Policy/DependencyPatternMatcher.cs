namespace CSharpDoctor.Architecture.Policy;

internal static class DependencyPatternMatcher
{
    public static bool MatchesNamespace(string pattern, string value)
    {
        if (string.IsNullOrEmpty(pattern) || string.IsNullOrEmpty(value))
        {
            return false;
        }

        if (pattern.EndsWith('*'))
        {
            var prefix = pattern[..^1];
            return value.Equals(prefix, StringComparison.Ordinal)
                || value.StartsWith(prefix + ".", StringComparison.Ordinal);
        }

        return value.Equals(pattern, StringComparison.Ordinal)
            || value.StartsWith(pattern + ".", StringComparison.Ordinal);
    }

    public static bool MatchesAssembly(string pattern, string value)
    {
        if (string.IsNullOrEmpty(pattern) || string.IsNullOrEmpty(value))
        {
            return false;
        }

        if (pattern.EndsWith('*'))
        {
            return value.StartsWith(pattern[..^1], StringComparison.OrdinalIgnoreCase);
        }

        return string.Equals(pattern, value, StringComparison.OrdinalIgnoreCase);
    }
}
