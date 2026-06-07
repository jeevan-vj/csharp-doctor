namespace CSharpDoctor.Cli.IntegrationTests.Fixtures;

/// <summary>A project containing an EF Core migration that drops a column.</summary>
public sealed class EfMigrationFixture : IDisposable
{
    public string Root { get; }

    public string MigrationPath { get; }

    public EfMigrationFixture(bool includeMigration = true)
    {
        Root = Path.Combine(Path.GetTempPath(), "csharp-doctor-ef-" + Guid.NewGuid().ToString("N"));
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

        MigrationPath = Path.Combine(appDir, "Migrations", "20260101000000_RemoveLegacy.cs");

        if (includeMigration)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(MigrationPath)!);
            File.WriteAllText(
                MigrationPath,
                """
                using Microsoft.EntityFrameworkCore.Migrations;

                namespace App.Migrations;

                public partial class RemoveLegacy : Migration
                {
                    protected override void Up(MigrationBuilder migrationBuilder)
                    {
                        migrationBuilder.DropColumn(name: "OldCol", table: "Users");
                    }

                    protected override void Down(MigrationBuilder migrationBuilder)
                    {
                    }
                }
                """);
        }
        else
        {
            File.WriteAllText(
                Path.Combine(appDir, "Class1.cs"),
                "namespace App; public class Class1 { }");
        }
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
