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
        var merged = NormalizeLegacyAliases(Merge(fileConfig, configDir));
        return ApplyCliOverrides(merged, cliOverrides);
    }

    /// <summary>Folds the deprecated top-level <c>lint</c> flag into <see cref="ChecksConfig"/>.</summary>
    public static CSharpDoctorConfig NormalizeLegacyAliases(CSharpDoctorConfig config)
    {
        if (!config.Lint && config.Checks.Lint)
        {
            return config with { Checks = config.Checks with { Lint = false } };
        }

        return config;
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
            Checks = config.Checks with
            {
                Lint = cli.Lint ?? config.Checks.Lint,
                Wiring = cli.Wiring ?? config.Checks.Wiring,
                Ef = cli.Ef ?? config.Checks.Ef,
                Architecture = cli.Architecture ?? config.Checks.Architecture,
                Sca = cli.Sca ?? config.Checks.Sca,
            },
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

                CSharpDoctorConfig? parsed;
                try
                {
                    parsed = JsonSerializer.Deserialize<CSharpDoctorConfig>(json, options);
                }
                catch (JsonException ex)
                {
                    throw new InvalidOperationException(
                        $"Invalid '{ConfigFileName}' at '{configPath}': {ex.Message} "
                        + "Valid 'checks' keys are: lint, wiring, ef, architecture, sca.",
                        ex);
                }

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
    bool? Wiring = null,
    bool? Ef = null,
    bool? Architecture = null,
    bool? Sca = null,
    bool? DeadCode = null,
    FailOnLevel? FailOn = null,
    string? RootDir = null,
    bool? RespectInlineDisables = null,
    IReadOnlyDictionary<string, SeverityOverride>? Rules = null,
    IReadOnlyDictionary<string, SeverityOverride>? Categories = null);
