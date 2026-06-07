namespace CSharpDoctor.Wiring.Registration;

/// <summary>Normalizes type names for registration matching.</summary>
public static class WiringTypeIdentity
{
    public static string Normalize(string typeName)
    {
        var trimmed = typeName.Trim();
        var genericIndex = trimmed.IndexOf('<', StringComparison.Ordinal);
        if (genericIndex > 0)
        {
            trimmed = trimmed[..genericIndex];
        }

        var dot = trimmed.LastIndexOf('.');
        return dot >= 0 ? trimmed[(dot + 1)..] : trimmed;
    }

    public static bool IsFrameworkProvided(string typeName)
    {
        var normalized = Normalize(typeName);
        if (normalized is "ILogger" or "IConfiguration" or "IServiceProvider" or "IServiceScopeFactory"
            or "IHostEnvironment" or "IWebHostEnvironment" or "IHostApplicationLifetime"
            or "IHttpContextAccessor" or "HttpContext")
        {
            return true;
        }

        if (normalized.StartsWith("ILogger<", StringComparison.Ordinal)
            || normalized.StartsWith("IOptions", StringComparison.Ordinal)
            || normalized.StartsWith("IEnumerable<", StringComparison.Ordinal)
            || normalized.StartsWith("IReadOnlyList<", StringComparison.Ordinal))
        {
            return true;
        }

        return false;
    }
}
