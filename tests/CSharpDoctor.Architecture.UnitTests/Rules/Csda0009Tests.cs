using CSharpDoctor.Architecture.Rules;
using Shouldly;
using Xunit;

namespace CSharpDoctor.Architecture.UnitTests.Rules;

public sealed class Csda0009Tests : IDisposable
{
    private readonly string _root;

    public Csda0009Tests()
    {
        _root = Path.Combine(Path.GetTempPath(), "csda0009-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_root);
    }

    [Fact]
    public void Run_EmitsDiagnostic_WhenCompileIncludeUsesParentPath()
    {
        var projectDir = Path.Combine(_root, "App");
        Directory.CreateDirectory(projectDir);
        var csproj = Path.Combine(projectDir, "App.csproj");
        File.WriteAllText(
            csproj,
            """
            <Project Sdk="Microsoft.NET.Sdk">
              <ItemGroup>
                <Compile Include="..\Shared\Outside.cs" />
              </ItemGroup>
            </Project>
            """);

        var diagnostics = Csda0009SourceFileOutsideProject.Run([csproj], CancellationToken.None).ToList();

        diagnostics.ShouldHaveSingleItem();
        diagnostics[0].Rule.ShouldBe("CSDA0009");
    }

    [Fact]
    public void Run_DoesNotEmit_WhenCompileIncludeIsLocal()
    {
        var projectDir = Path.Combine(_root, "App");
        Directory.CreateDirectory(projectDir);
        var csproj = Path.Combine(projectDir, "App.csproj");
        File.WriteAllText(
            csproj,
            """
            <Project Sdk="Microsoft.NET.Sdk">
              <ItemGroup>
                <Compile Include="Program.cs" />
              </ItemGroup>
            </Project>
            """);

        var diagnostics = Csda0009SourceFileOutsideProject.Run([csproj], CancellationToken.None);

        diagnostics.ShouldBeEmpty();
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
