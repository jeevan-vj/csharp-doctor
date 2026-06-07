namespace CSharpDoctor.Cli.IntegrationTests.Fixtures;

public sealed class NamespaceCycleFixture : IDisposable
{
    public string Root { get; }

    public NamespaceCycleFixture()
    {
        Root = Path.Combine(Path.GetTempPath(), "csharp-doctor-arch-" + Guid.NewGuid().ToString("N"));
        var appDir = Path.Combine(Root, "App");
        Directory.CreateDirectory(appDir);

        File.WriteAllText(
            Path.Combine(appDir, "App.csproj"),
            """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net10.0</TargetFramework>
                <Nullable>enable</Nullable>
              </PropertyGroup>
            </Project>
            """);

        File.WriteAllText(
            Path.Combine(appDir, "A.cs"),
            """
            namespace App.A;

            public class TypeA
            {
                public App.B.TypeB Create() => new();
            }
            """);

        File.WriteAllText(
            Path.Combine(appDir, "B.cs"),
            """
            namespace App.B;

            public class TypeB
            {
                public App.A.TypeA Create() => new();
            }
            """);

        File.WriteAllText(
            Path.Combine(Root, "App.sln"),
            """
            Microsoft Visual Studio Solution File, Format Version 12.00
            Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "App", "App\App.csproj", "{DDDDDDDD-DDDD-DDDD-DDDD-DDDDDDDDDDDD}"
            EndProject
            Global
            	GlobalSection(SolutionConfigurationPlatforms) = preSolution
            		Debug|Any CPU = Debug|Any CPU
            	EndGlobalSection
            	GlobalSection(ProjectConfigurationPlatforms) = postSolution
            		{DDDDDDDD-DDDD-DDDD-DDDD-DDDDDDDDDDDD}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
            		{DDDDDDDD-DDDD-DDDD-DDDD-DDDDDDDDDDDD}.Debug|Any CPU.Build.0 = Debug|Any CPU
            	EndGlobalSection
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
