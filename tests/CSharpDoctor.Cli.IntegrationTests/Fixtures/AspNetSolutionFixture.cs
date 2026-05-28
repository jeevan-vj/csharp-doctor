namespace CSharpDoctor.Cli.IntegrationTests.Fixtures;

/// <summary>ASP.NET Core web project fixture for integration tests.</summary>
public sealed class AspNetSolutionFixture : IDisposable
{
    public string Root { get; }

    public string ProjectPath { get; }

    public string ProgramPath { get; }

    public AspNetSolutionFixture()
    {
        Root = Path.Combine(Path.GetTempPath(), "csharp-doctor-aspnet-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Root);

        ProjectPath = Path.Combine(Root, "WebApp", "WebApp.csproj");
        Directory.CreateDirectory(Path.GetDirectoryName(ProjectPath)!);

        File.WriteAllText(
            ProjectPath,
            """
            <Project Sdk="Microsoft.NET.Sdk.Web">
              <PropertyGroup>
                <TargetFramework>net10.0</TargetFramework>
                <Nullable>enable</Nullable>
              </PropertyGroup>
              <ItemGroup>
                <PackageReference Include="Microsoft.EntityFrameworkCore" Version="10.0.0" />
              </ItemGroup>
            </Project>
            """);

        ProgramPath = Path.Combine(Root, "WebApp", "Program.cs");
        File.WriteAllText(
            ProgramPath,
            """
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddSingleton<AppDbContext>();
            builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowCredentials()));
            builder.Services.AddControllers().AddNewtonsoftJson();
            var app = builder.Build();
            app.MapGet("/api/data", () => "ok");
            app.Run();

            class AppDbContext { }
            """);

        File.WriteAllText(
            Path.Combine(Root, "WebApp.sln"),
            """
            Microsoft Visual Studio Solution File, Format Version 12.00
            Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "WebApp", "WebApp\WebApp.csproj", "{22222222-2222-2222-2222-222222222222}"
            EndProject
            Global
              GlobalSection(SolutionConfigurationPlatforms) = preSolution
                Debug|Any CPU = Debug|Any CPU
              EndSection
              GlobalSection(ProjectConfigurationPlatforms) = postSolution
                {22222222-2222-2222-2222-222222222222}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
                {22222222-2222-2222-2222-222222222222}.Debug|Any CPU.Build.0 = Debug|Any CPU
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
