using System.Text.Json;

namespace CSharpDoctor.Sca;

public sealed record VulnerablePackage(
    string ProjectPath,
    string PackageId,
    string ResolvedVersion,
    bool IsTransitive,
    string Severity,
    string AdvisoryUrl);

public static class VulnerablePackageReportParser
{
    public static IReadOnlyList<VulnerablePackage> Parse(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        using var document = JsonDocument.Parse(json);
        if (!document.RootElement.TryGetProperty("projects", out var projects))
        {
            return [];
        }

        var results = new List<VulnerablePackage>();
        foreach (var project in projects.EnumerateArray())
        {
            var projectPath = project.TryGetProperty("path", out var pathElement)
                ? pathElement.GetString() ?? string.Empty
                : string.Empty;

            if (!project.TryGetProperty("frameworks", out var frameworks))
            {
                continue;
            }

            foreach (var framework in frameworks.EnumerateArray())
            {
                CollectPackages(projectPath, framework, "topLevelPackages", isTransitive: false, results);
                CollectPackages(projectPath, framework, "transitivePackages", isTransitive: true, results);
            }
        }

        return results;
    }

    private static void CollectPackages(
        string projectPath,
        JsonElement framework,
        string propertyName,
        bool isTransitive,
        List<VulnerablePackage> results)
    {
        if (!framework.TryGetProperty(propertyName, out var packages))
        {
            return;
        }

        foreach (var package in packages.EnumerateArray())
        {
            if (!package.TryGetProperty("vulnerabilities", out var vulnerabilities)
                || vulnerabilities.GetArrayLength() == 0)
            {
                continue;
            }

            var id = package.TryGetProperty("id", out var idElement)
                ? idElement.GetString() ?? string.Empty
                : string.Empty;
            var version = package.TryGetProperty("resolvedVersion", out var versionElement)
                ? versionElement.GetString() ?? string.Empty
                : string.Empty;

            foreach (var vulnerability in vulnerabilities.EnumerateArray())
            {
                var severity = vulnerability.TryGetProperty("severity", out var severityElement)
                    ? severityElement.GetString() ?? "Unknown"
                    : "Unknown";
                var advisoryUrl = vulnerability.TryGetProperty("advisoryurl", out var urlElement)
                    ? urlElement.GetString() ?? string.Empty
                    : string.Empty;

                results.Add(new VulnerablePackage(
                    projectPath,
                    id,
                    version,
                    isTransitive,
                    severity,
                    advisoryUrl));
            }
        }
    }
}
