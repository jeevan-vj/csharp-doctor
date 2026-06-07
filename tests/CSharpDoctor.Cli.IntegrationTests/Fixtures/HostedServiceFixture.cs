namespace CSharpDoctor.Cli.IntegrationTests.Fixtures;

/// <summary>
/// Two-project solution: a Web project whose composition root may or may not register a
/// <c>BackgroundService</c> declared in a separate Workers project.
/// </summary>
public sealed class HostedServiceFixture : IDisposable
{
    public string Root { get; }

    public string ProgramPath { get; }

    public string WorkerPath { get; }

    public HostedServiceFixture()
    {
        Root = Path.Combine(Path.GetTempPath(), "csharp-doctor-wiring-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Root);

        var workersDir = Path.Combine(Root, "Workers");
        var webDir = Path.Combine(Root, "Web");
        Directory.CreateDirectory(workersDir);
        Directory.CreateDirectory(webDir);

        File.WriteAllText(
            Path.Combine(workersDir, "Workers.csproj"),
            """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net10.0</TargetFramework>
                <Nullable>enable</Nullable>
              </PropertyGroup>
            </Project>
            """);

        WorkerPath = Path.Combine(workersDir, "Worker.cs");
        File.WriteAllText(
            WorkerPath,
            """
            namespace Workers;

            public class Worker : BackgroundService
            {
                protected override Task ExecuteAsync(CancellationToken stoppingToken) => Task.CompletedTask;
            }
            """);

        File.WriteAllText(
            Path.Combine(webDir, "Web.csproj"),
            """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net10.0</TargetFramework>
                <Nullable>enable</Nullable>
              </PropertyGroup>
              <ItemGroup>
                <ProjectReference Include="..\Workers\Workers.csproj" />
              </ItemGroup>
            </Project>
            """);

        ProgramPath = Path.Combine(webDir, "Program.cs");
        WriteUnregisteredProgram();

        File.WriteAllText(
            Path.Combine(Root, "App.sln"),
            """
            Microsoft Visual Studio Solution File, Format Version 12.00
            Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "Workers", "Workers\Workers.csproj", "{AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA}"
            EndProject
            Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "Web", "Web\Web.csproj", "{BBBBBBBB-BBBB-BBBB-BBBB-BBBBBBBBBBBB}"
            EndProject
            Global
            	GlobalSection(SolutionConfigurationPlatforms) = preSolution
            		Debug|Any CPU = Debug|Any CPU
            	EndGlobalSection
            	GlobalSection(ProjectConfigurationPlatforms) = postSolution
            		{AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
            		{AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA}.Debug|Any CPU.Build.0 = Debug|Any CPU
            		{BBBBBBBB-BBBB-BBBB-BBBB-BBBBBBBBBBBB}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
            		{BBBBBBBB-BBBB-BBBB-BBBB-BBBBBBBBBBBB}.Debug|Any CPU.Build.0 = Debug|Any CPU
            	EndGlobalSection
            EndGlobal
            """);
    }

    public void WriteUnregisteredProgram() =>
        File.WriteAllText(
            ProgramPath,
            """
            using Workers;

            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddSingleton<Worker>();
            var app = builder.Build();
            app.Run();
            """);

    public void WriteRegisteredProgram() =>
        File.WriteAllText(
            ProgramPath,
            """
            using Workers;

            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddHostedService<Worker>();
            var app = builder.Build();
            app.Run();
            """);

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
