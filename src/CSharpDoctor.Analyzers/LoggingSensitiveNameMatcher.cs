namespace CSharpDoctor.Analyzers;

/// <summary>Matches sensitive names in logging templates and property names.</summary>
public static class LoggingSensitiveNameMatcher
{
    private static readonly string[] SensitiveTokens =
    [
        "password", "passwd", "secret", "token", "accesstoken", "refreshtoken",
        "authorization", "cookie", "set-cookie", "apikey", "ssn", "creditcard",
    ];

    private static readonly string[] SafePrefixes =
    [
        "redacted", "masked", "hashed", "hash",
    ];

    public static bool IsSensitiveName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        var value = name!;
        var normalized = value.Replace("-", string.Empty)
            .Replace("_", string.Empty);

        if (SafePrefixes.Any(p => normalized.Contains(p, StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        return SensitiveTokens.Any(token =>
            normalized.Contains(token, StringComparison.OrdinalIgnoreCase));
    }

    public static bool IsEventNameOnly(string name) =>
        name.EndsWith("Changed", StringComparison.OrdinalIgnoreCase)
        || name.EndsWith("Updated", StringComparison.OrdinalIgnoreCase)
        || name.EndsWith("Reset", StringComparison.OrdinalIgnoreCase);
}
