using CSharpDoctor.Core.Discovery;
using Shouldly;
using Xunit;

namespace CSharpDoctor.Core.UnitTests.Discovery;

public sealed class ProjectDiscoveryTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "csd-discovery-" + Guid.NewGuid().ToString("N"));

    public ProjectDiscoveryTests() => Directory.CreateDirectory(_root);

    [Fact]
    public void FindProjects_PrefersSlnx_OverLooseCsproj()
    {
        WriteProject("CommonAsp/CommonAsp.csproj");
        WriteProject("MediasaucierCore/MediasaucierCore.csproj");
        File.WriteAllText(
            Path.Combine(_root, "App.slnx"),
            """
            <Solution>
              <Project Path="MediasaucierCore/MediasaucierCore.csproj" />
              <Folder Name="/apps/">
                <Project Path="CommonAsp/CommonAsp.csproj" />
              </Folder>
            </Solution>
            """);

        var projects = ProjectDiscovery.FindProjects(_root);

        projects.Count.ShouldBe(1);
        projects[0].ShouldEndWith("App.slnx");
    }

    [Fact]
    public void FindProjects_PrefersShallowerSolution_OverNestedOne()
    {
        Directory.CreateDirectory(Path.Combine(_root, "nested"));
        File.WriteAllText(Path.Combine(_root, "nested", "Nested.sln"), " ");
        File.WriteAllText(Path.Combine(_root, "App.slnx"), "<Solution />");

        var projects = ProjectDiscovery.FindProjects(_root);

        projects[0].ShouldEndWith("App.slnx");
    }

    [Fact]
    public void ReadSlnxProjectPaths_ResolvesRelativePaths_IncludingNestedFolders()
    {
        var slnx = Path.Combine(_root, "App.slnx");
        File.WriteAllText(
            slnx,
            """
            <Solution>
              <Project Path="src/Api/Api.csproj" />
              <Folder Name="/src/">
                <Project Path="src/Core/Core.csproj" />
              </Folder>
              <Folder Name="/items/">
                <File Path="README.md" />
              </Folder>
            </Solution>
            """);

        var paths = ProjectDiscovery.ReadSlnxProjectPaths(slnx);

        paths.Count.ShouldBe(2);
        paths.ShouldContain(Path.Combine(_root, "src", "Api", "Api.csproj"));
        paths.ShouldContain(Path.Combine(_root, "src", "Core", "Core.csproj"));
    }

    [Fact]
    public void ResolveScanRoot_KeepsTheDirectoryPassedIn()
    {
        WriteProject("CommonAsp/CommonAsp.csproj");
        WriteProject("MediasaucierCore/MediasaucierCore.csproj");
        File.WriteAllText(Path.Combine(_root, "CommonAsp", "GetIpService.cs"), "class GetIpService { }");
        File.WriteAllText(Path.Combine(_root, "MediasaucierCore", "Program.cs"), "class Program { }");

        ProjectDiscovery.ResolveScanRoot(_root).ShouldBe(Path.GetFullPath(_root));

        var sources = ProjectDiscovery.FindSourceFiles(ProjectDiscovery.ResolveScanRoot(_root));
        sources.Count.ShouldBe(2);
    }

    public void Dispose()
    {
        try
        {
            Directory.Delete(_root, recursive: true);
        }
        catch (IOException)
        {
            // best-effort cleanup
        }
    }

    private void WriteProject(string relativePath)
    {
        var path = Path.Combine(_root, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(
            path,
            """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup><TargetFramework>net10.0</TargetFramework></PropertyGroup>
            </Project>
            """);
    }
}
