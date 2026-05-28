using System.Text.Json;
using System.Text.Json.Serialization;

namespace CSharpDoctor.Core.Configuration;

/// <summary>Merges CLI overrides, file config, and defaults.</summary>
public static class ConfigResolver
{
    public const string ConfigFileName = ".csharp-doctor.json";

    public static CSharpDoctorConfig Resolve(
        string? directory,
        CliConfigOverrides? cliOverrides = null,
        CancellationToken cancellationToken = default)
    {
        var (fileConfig, configDir) = LoadFromDirectory(directory, cancellationToken);
        var merged = Merge(fileConfig, configDir);
        return ApplyCliOverrides(merged, cliOverrides);
    }

    public static CSharpDoctorConfig Merge(
        CSharpDoctorConfig? fileConfig,
        string? configFileDirectory)
    {
        if (fileConfig is null)
        {
            return new CSharpDoctorConfig { ConfigFileDirectory = configFileDirectory };
        }

        return fileConfig with { ConfigFileDirectory = configFileDirectory ?? fileConfig.ConfigFileDirectory };
    }

    public static CSharpDoctorConfig ApplyCliOverrides(
        CSharpDoctorConfig config,
        CliConfigOverrides? cli)
    {
        if (cli is null)
        {
            return config;
        }

        return config with
        {
            Lint = cli.Lint ?? config.Lint,
            DeadCode = cli.DeadCode ?? config.DeadCode,
            FailOn = cli.FailOn ?? config.FailOn,
            RootDir = cli.RootDir ?? config.RootDir,
            RespectInlineDisables = cli.RespectInlineDisables ?? config.RespectInlineDisables,
            Rules = cli.Rules is not null
                ? MergeDictionary(config.Rules, cli.Rules)
                : config.Rules,
            Categories = cli.Categories is not null
                ? MergeDictionary(config.Categories, cli.Categories)
                : config.Categories,
        };
    }

    public static string ResolvePathRelativeToConfig(string? configDirectory, string relativePath)
    {
        if (string.IsNullOrWhiteSpace(configDirectory))
        {
            return relativePath;
        }

        return Path.GetFullPath(Path.Combine(configDirectory, relativePath));
    }

    private static (CSharpDoctorConfig? Config, string? Directory) LoadFromDirectory(
        string? directory,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(directory))
        {
            return (null, null);
        }

        var current = new DirectoryInfo(Path.GetFullPath(directory));
        while (current is not null)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var configPath = Path.Combine(current.FullName, ConfigFileName);
            if (File.Exists(configPath))
            {
                var json = File.ReadAllText(configPath);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
                };
                var parsed = JsonSerializer.Deserialize<CSharpDoctorConfig>(json, options);
                return (parsed, current.FullName);
            }

            if (IsProjectBoundary(current))
            {
                break;
            }

            current = current.Parent;
        }

        return (null, Path.GetFullPath(directory));
    }

    private static bool IsProjectBoundary(DirectoryInfo directory) =>
        directory.GetFiles("*.sln").Length > 0
        || directory.GetFiles(".git").Length > 0
        || (directory.Parent is null);

    private static IReadOnlyDictionary<string, SeverityOverride> MergeDictionary(
        IReadOnlyDictionary<string, SeverityOverride> baseline,
        IReadOnlyDictionary<string, SeverityOverride> overrides)
    {
        var result = new Dictionary<string, SeverityOverride>(baseline, StringComparer.OrdinalIgnoreCase);
        foreach (var (key, value) in overrides)
        {
            result[key] = value;
        }

        return result;
    }
}

/// <summary>CLI flag overrides applied after file config.</summary>
public sealed record CliConfigOverrides(
    bool? Lint = null,
    bool? DeadCode = null,
    FailOnLevel? FailOn = null,
    string? RootDir = null,
    bool? RespectInlineDisables = null,
    IReadOnlyDictionary<string, SeverityOverride>? Rules = null,
    IReadOnlyDictionary<string, SeverityOverride>? Categories = null);
