namespace CSharpDoctor.Core.Discovery;

/// <summary>Capabilities inferred from a .csproj (package and SDK references).</summary>
public sealed record ProjectCapabilities(bool AspNetCore, bool EntityFrameworkCore)
{
    public static ProjectCapabilities None { get; } = new(false, false);

    public bool Has(string capability) =>
        capability switch
        {
            "aspnet" => AspNetCore,
            "efcore" => EntityFrameworkCore,
            _ => true,
        };

    public static ProjectCapabilities FromCsproj(string csprojPath)
    {
        if (!File.Exists(csprojPath))
        {
            return None;
        }

        var text = File.ReadAllText(csprojPath);
        var aspNet = text.Contains("Microsoft.AspNetCore", StringComparison.OrdinalIgnoreCase)
            || text.Contains("Microsoft.NET.Sdk.Web", StringComparison.OrdinalIgnoreCase);
        var ef = text.Contains("Microsoft.EntityFrameworkCore", StringComparison.OrdinalIgnoreCase);
        return new(aspNet, ef);
    }
}
