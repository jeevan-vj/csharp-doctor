namespace CSharpDoctor.Cli.IntegrationTests.Fixtures;

/// <summary>Creates temporary .NET solutions for integration tests.</summary>
public sealed class TempSolutionFixture : IDisposable
{
    public string Root { get; }

    public string ProjectPath { get; }

    public string BadCodePath { get; }

    public TempSolutionFixture()
    {
        Root = Path.Combine(Path.GetTempPath(), "csharp-doctor-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Root);

        ProjectPath = Path.Combine(Root, "Sample", "Sample.csproj");
        Directory.CreateDirectory(Path.GetDirectoryName(ProjectPath)!);

        File.WriteAllText(
            ProjectPath,
            """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net10.0</TargetFramework>
                <Nullable>enable</Nullable>
              </PropertyGroup>
            </Project>
            """);

        BadCodePath = Path.Combine(Root, "Sample", "Bad.cs");
        File.WriteAllText(
            BadCodePath,
            """
            using System.Security.Cryptography;
            class SqlCommand { public string CommandText { get; set; } }
            class Bad {
                void M(string input) {
                    var cmd = new SqlCommand();
                    cmd.CommandText = "SELECT * FROM t WHERE id = " + input;
                    var h = MD5.Create();
                }
            }
            """);

        File.WriteAllText(
            Path.Combine(Root, "Sample.sln"),
            """
            Microsoft Visual Studio Solution File, Format Version 12.00
            Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "Sample", "Sample\Sample.csproj", "{11111111-1111-1111-1111-111111111111}"
            EndProject
            Global
              GlobalSection(SolutionConfigurationPlatforms) = preSolution
                Debug|Any CPU = Debug|Any CPU
              EndSection
              GlobalSection(ProjectConfigurationPlatforms) = postSolution
                {11111111-1111-1111-1111-111111111111}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
                {11111111-1111-1111-1111-111111111111}.Debug|Any CPU.Build.0 = Debug|Any CPU
              EndSection
            EndGlobal
            """);
    }

    public void Dispose()
    {
        try
        {
            Directory.Delete(Root, recursive: true);
        }
        catch
        {
            // best-effort cleanup
        }
    }
}
