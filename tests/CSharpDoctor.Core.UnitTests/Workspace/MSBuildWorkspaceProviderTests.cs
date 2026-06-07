using CSharpDoctor.Core.Workspace;
using Shouldly;
using Xunit;

namespace CSharpDoctor.Core.UnitTests.Workspace;

[Collection("msbuild")]
public sealed class MSBuildWorkspaceProviderTests : IDisposable
{
    private readonly string _root;

    public MSBuildWorkspaceProviderTests()
    {
        _root = Path.Combine(Path.GetTempPath(), "csd-ws-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_root);
    }

    [Fact]
    public async Task LoadAsync_WithSolution_ReturnsSolutionWithProject()
    {
        WriteSampleSolution();
        var diagnostics = new StringWriter();
        using var provider = new MSBuildWorkspaceProvider(diagnostics);

        var solution = await provider.LoadAsync(_root, CancellationToken.None);

        solution.ShouldNotBeNull();
        solution!.Projects.Count().ShouldBe(1, diagnostics.ToString());
    }

    [Fact]
    public async Task LoadAsync_CalledTwice_IsIdempotent()
    {
        WriteSampleSolution();
        using var provider = new MSBuildWorkspaceProvider();

        var first = await provider.LoadAsync(_root, CancellationToken.None);
        var second = await provider.LoadAsync(_root, CancellationToken.None);

        second.ShouldBeSameAs(first);
    }

    [Fact]
    public async Task LoadAsync_WithNoProjects_ReturnsNull()
    {
        using var provider = new MSBuildWorkspaceProvider();

        var solution = await provider.LoadAsync(_root, CancellationToken.None);

        solution.ShouldBeNull();
    }

    private void WriteSampleSolution()
    {
        var projectPath = Path.Combine(_root, "Sample", "Sample.csproj");
        Directory.CreateDirectory(Path.GetDirectoryName(projectPath)!);
        File.WriteAllText(
            projectPath,
            """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net10.0</TargetFramework>
                <Nullable>enable</Nullable>
              </PropertyGroup>
            </Project>
            """);

        File.WriteAllText(Path.Combine(_root, "Sample", "Class1.cs"), "namespace Sample; public class Class1 { }");

        File.WriteAllText(
            Path.Combine(_root, "Sample.sln"),
            """
            Microsoft Visual Studio Solution File, Format Version 12.00
            Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "Sample", "Sample\Sample.csproj", "{11111111-1111-1111-1111-111111111111}"
            EndProject
            Global
            	GlobalSection(SolutionConfigurationPlatforms) = preSolution
            		Debug|Any CPU = Debug|Any CPU
            	EndGlobalSection
            	GlobalSection(ProjectConfigurationPlatforms) = postSolution
            		{11111111-1111-1111-1111-111111111111}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
            		{11111111-1111-1111-1111-111111111111}.Debug|Any CPU.Build.0 = Debug|Any CPU
            	EndGlobalSection
            EndGlobal
            """);
    }

    public void Dispose()
    {
        try
        {
            Directory.Delete(_root, recursive: true);
        }
        catch
        {
            // best-effort cleanup
        }
    }
}

[CollectionDefinition("msbuild", DisableParallelization = true)]
public sealed class MSBuildCollection;
