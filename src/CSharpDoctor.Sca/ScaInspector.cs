using CSharpDoctor.Core.Diagnostics;
using CSharpDoctor.Core.Inspection;
using CSharpDoctor.Sca.Rules;

namespace CSharpDoctor.Sca;

/// <summary>Scans NuGet dependencies for known vulnerabilities via <c>dotnet list package --vulnerable</c>.</summary>
public sealed class ScaInspector : IInspector
{
    private readonly IVulnerablePackageScanner _scanner;

    public ScaInspector()
        : this(new DotNetVulnerablePackageScanner())
    {
    }

    public ScaInspector(IVulnerablePackageScanner scanner) => _scanner = scanner;

    public string Name => InspectorRegistry.ScaInspectorName;

    public async Task<IReadOnlyList<DiagnosticRecord>> InspectAsync(
        InspectContext context,
        CancellationToken cancellationToken = default)
    {
        var packages = new List<VulnerablePackage>();
        foreach (var csproj in context.CsprojPaths)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var json = await _scanner.ScanProjectAsync(csproj, cancellationToken).ConfigureAwait(false);
            if (json is null)
            {
                continue;
            }

            packages.AddRange(VulnerablePackageReportParser.Parse(json));
        }

        var diagnostics = new List<DiagnosticRecord>();
        diagnostics.AddRange(Csds0001VulnerableDirectPackage.Run(packages));
        diagnostics.AddRange(Csds0002VulnerableTransitivePackage.Run(packages));
        return diagnostics;
    }
}
