using CSharpDoctor.Core.Configuration;
using CSharpDoctor.Core.Discovery;
using CSharpDoctor.Core.Inspection;
using CSharpDoctor.Sca;
using Microsoft.CodeAnalysis;
using Shouldly;
using Xunit;

namespace CSharpDoctor.Cli.IntegrationTests;

public sealed class ScaInspectorTests
{
    [Fact]
    public async Task Inspect_WithFixtureJson_EmitsCsds0002_ForTransitiveVulnerability()
    {
        var fixtureJson = """
            {
              "projects": [
                {
                  "path": "/app/Test.csproj",
                  "frameworks": [
                    {
                      "framework": "net10.0",
                      "transitivePackages": [
                        {
                          "id": "Microsoft.Build.Tasks.Core",
                          "resolvedVersion": "17.7.2",
                          "vulnerabilities": [
                            {
                              "severity": "High",
                              "advisoryurl": "https://github.com/advisories/GHSA-h4j7-5rxr-p4wc"
                            }
                          ]
                        }
                      ]
                    }
                  ]
                }
              ]
            }
            """;

        var scanner = new FakeScanner(fixtureJson);
        var inspector = new ScaInspector(scanner);
        var context = new InspectContext(
            new InspectRequest(Environment.CurrentDirectory),
            new CSharpDoctorConfig { Checks = new ChecksConfig { Sca = true } },
            SourceFiles: [],
            CsprojPaths: ["/app/Test.csproj"],
            ScanRoot: Environment.CurrentDirectory,
            Capabilities: new Dictionary<string, ProjectCapabilities>(),
            SolutionLazy: new Lazy<Task<Solution?>>(() => Task.FromResult<Solution?>(null)));

        var diagnostics = await inspector.InspectAsync(context);

        diagnostics.ShouldContain(d => d.Rule == "CSDS0002");
    }

    private sealed class FakeScanner(string json) : IVulnerablePackageScanner
    {
        public Task<string?> ScanProjectAsync(string csprojPath, CancellationToken cancellationToken) =>
            Task.FromResult<string?>(json);
    }
}
