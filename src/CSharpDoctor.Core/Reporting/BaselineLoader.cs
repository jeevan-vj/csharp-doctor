using System.Text.Json;

namespace CSharpDoctor.Core.Reporting;

/// <summary>Loads diagnostic identities from a previous JSON report for new-issue gates.</summary>
public static class BaselineLoader
{
    public static IReadOnlySet<string>? LoadIdentities(string? baselinePath)
    {
        if (string.IsNullOrWhiteSpace(baselinePath) || !File.Exists(baselinePath))
        {
            return null;
        }

        using var document = JsonDocument.Parse(File.ReadAllText(baselinePath));
        if (!document.RootElement.TryGetProperty("diagnostics", out var diagnostics))
        {
            return null;
        }

        var identities = new HashSet<string>(StringComparer.Ordinal);
        foreach (var diagnostic in diagnostics.EnumerateArray())
        {
            if (diagnostic.TryGetProperty("identity", out var identity)
                && identity.GetString() is { Length: > 0 } value)
            {
                identities.Add(value);
            }
        }

        return identities;
    }
}
