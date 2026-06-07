namespace CSharpDoctor.Cli.IntegrationTests.Fixtures;

/// <summary>Fixture for DI graph wiring rules (CSDW0008, CSDW0010).</summary>
public sealed class DiGraphFixture : IDisposable
{
    public string Root { get; }

    public string ProgramPath { get; }

    public string OrderServicePath { get; }

    public DiGraphFixture()
    {
        Root = Path.Combine(Path.GetTempPath(), "csharp-doctor-digraph-" + Guid.NewGuid().ToString("N"));
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

        File.WriteAllText(Path.Combine(appDir, "IFoo.cs"), "namespace App; public interface IFoo { }");
        File.WriteAllText(Path.Combine(appDir, "FooImpl.cs"), "namespace App; public class FooImpl : IFoo { }");

        OrderServicePath = Path.Combine(appDir, "OrderService.cs");
        File.WriteAllText(
            OrderServicePath,
            """
            namespace App;

            public class OrderService
            {
                public OrderService(IFoo foo) { }
            }
            """);

        ProgramPath = Path.Combine(appDir, "Program.cs");
        WriteUnregisteredProgram();

        File.WriteAllText(
            Path.Combine(Root, "App.sln"),
            """
            Microsoft Visual Studio Solution File, Format Version 12.00
            Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "App", "App\App.csproj", "{CCCCCCCC-CCCC-CCCC-CCCC-CCCCCCCCCCCC}"
            EndProject
            Global
            	GlobalSection(SolutionConfigurationPlatforms) = preSolution
            		Debug|Any CPU = Debug|Any CPU
            	EndGlobalSection
            	GlobalSection(ProjectConfigurationPlatforms) = postSolution
            		{CCCCCCCC-CCCC-CCCC-CCCC-CCCCCCCCCCCC}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
            		{CCCCCCCC-CCCC-CCCC-CCCC-CCCCCCCCCCCC}.Debug|Any CPU.Build.0 = Debug|Any CPU
            	EndGlobalSection
            EndGlobal
            """);
    }

    public void WriteUnregisteredProgram() =>
        File.WriteAllText(
            ProgramPath,
            """
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddScoped<OrderService>();
            var app = builder.Build();
            app.Run();
            """);

    public void WriteRegisteredProgram() =>
        File.WriteAllText(
            ProgramPath,
            """
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddScoped<IFoo, FooImpl>();
            builder.Services.AddScoped<OrderService>();
            var app = builder.Build();
            app.Run();
            """);

    public void WriteFrameworkServiceProgram() =>
        File.WriteAllText(
            OrderServicePath,
            """
            namespace App;

            public class OrderService
            {
                public OrderService(Microsoft.Extensions.Logging.ILogger<OrderService> logger) { }
            }
            """);

    public void WriteCaptiveDependencyProgram()
    {
        File.WriteAllText(
            Path.Combine(Path.GetDirectoryName(OrderServicePath)!, "AppDbContext.cs"),
            "namespace App; public class AppDbContext { }");

        File.WriteAllText(
            Path.Combine(Path.GetDirectoryName(OrderServicePath)!, "Worker.cs"),
            """
            namespace App;

            public class Worker
            {
                public Worker(AppDbContext db) { }
            }
            """);

        File.WriteAllText(
            ProgramPath,
            """
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddSingleton<Worker>();
            builder.Services.AddScoped<AppDbContext>();
            var app = builder.Build();
            app.Run();
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
