using CSharpDoctor.Sca;
using Shouldly;
using Xunit;

namespace CSharpDoctor.Sca.UnitTests;

public sealed class VulnerablePackageReportParserTests
{
    private const string SampleJson = """
        {
          "version": 1,
          "projects": [
            {
              "path": "/app/Api.csproj",
              "frameworks": [
                {
                  "framework": "net10.0",
                  "topLevelPackages": [
                    {
                      "id": "Vulnerable.Direct",
                      "resolvedVersion": "1.0.0",
                      "vulnerabilities": [
                        {
                          "severity": "High",
                          "advisoryurl": "https://github.com/advisories/GHSA-aaaa"
                        }
                      ]
                    }
                  ],
                  "transitivePackages": [
                    {
                      "id": "Vulnerable.Transitive",
                      "resolvedVersion": "2.0.0",
                      "vulnerabilities": [
                        {
                          "severity": "Moderate",
                          "advisoryurl": "https://github.com/advisories/GHSA-bbbb"
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

    [Fact]
    public void Parse_ReturnsDirectAndTransitivePackages()
    {
        var packages = VulnerablePackageReportParser.Parse(SampleJson);

        packages.Count.ShouldBe(2);
        packages.ShouldContain(p => p.PackageId == "Vulnerable.Direct" && !p.IsTransitive);
        packages.ShouldContain(p => p.PackageId == "Vulnerable.Transitive" && p.IsTransitive);
    }

    [Fact]
    public void Parse_MapsSeverityAndAdvisoryUrl()
    {
        var packages = VulnerablePackageReportParser.Parse(SampleJson);
        var direct = packages.Single(p => p.PackageId == "Vulnerable.Direct");

        direct.Severity.ShouldBe("High");
        direct.AdvisoryUrl.ShouldBe("https://github.com/advisories/GHSA-aaaa");
        direct.ProjectPath.ShouldBe("/app/Api.csproj");
    }

    [Fact]
    public void Parse_EmptyProjects_ReturnsEmpty()
    {
        VulnerablePackageReportParser.Parse("""{ "projects": [] }""").ShouldBeEmpty();
    }
}
