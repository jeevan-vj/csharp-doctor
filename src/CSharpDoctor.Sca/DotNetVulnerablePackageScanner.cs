using System.Diagnostics;
using System.Text;

namespace CSharpDoctor.Sca;

public interface IVulnerablePackageScanner
{
    Task<string?> ScanProjectAsync(string csprojPath, CancellationToken cancellationToken);
}

/// <summary>Runs <c>dotnet list package --vulnerable</c> for a single project.</summary>
public sealed class DotNetVulnerablePackageScanner : IVulnerablePackageScanner
{
    public async Task<string?> ScanProjectAsync(string csprojPath, CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"list \"{csprojPath}\" package --vulnerable --include-transitive --format json",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        using var process = new Process { StartInfo = startInfo };
        if (!process.Start())
        {
            return null;
        }

        var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var errorTask = process.StandardError.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);

        var output = await outputTask.ConfigureAwait(false);
        var error = await errorTask.ConfigureAwait(false);

        if (process.ExitCode != 0)
        {
            var message = new StringBuilder();
            message.AppendLine($"dotnet list package failed for '{csprojPath}' (exit {process.ExitCode}).");
            if (!string.IsNullOrWhiteSpace(error))
            {
                message.AppendLine(error.Trim());
            }

            throw new InvalidOperationException(message.ToString());
        }

        return string.IsNullOrWhiteSpace(output) ? null : output;
    }
}
