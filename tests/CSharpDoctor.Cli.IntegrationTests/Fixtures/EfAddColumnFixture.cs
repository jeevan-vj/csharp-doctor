namespace CSharpDoctor.Cli.IntegrationTests.Fixtures;

/// <summary>EF migration scenarios for CSDE0002 and CSDE0006.</summary>
public sealed class EfAddColumnFixture : IDisposable
{
    public string Root { get; }

    public string MigrationPath { get; }

    public EfAddColumnFixture(bool withDefault = false, bool nullable = false)
    {
        Root = Path.Combine(Path.GetTempPath(), "csharp-doctor-ef-addcol-" + Guid.NewGuid().ToString("N"));
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

        MigrationPath = Path.Combine(appDir, "Migrations", "20260101000000_AddScore.cs");
        Directory.CreateDirectory(Path.GetDirectoryName(MigrationPath)!);

        var nullableArg = nullable ? "true" : "false";
        var defaultArg = withDefault ? ", defaultValue: 0" : string.Empty;

        File.WriteAllText(
            MigrationPath,
            $$"""
            using Microsoft.EntityFrameworkCore.Migrations;

            namespace App.Migrations;

            public partial class AddScore : Migration
            {
                protected override void Up(MigrationBuilder migrationBuilder)
                {
                    migrationBuilder.AddColumn<int>(
                        name: "Score",
                        table: "Users",
                        type: "int",
                        nullable: {{nullableArg}}{{defaultArg}});
                }

                protected override void Down(MigrationBuilder migrationBuilder) { }
            }
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

public sealed class EfAlterColumnFixture : IDisposable
{
    public string Root { get; }

    public string MigrationPath { get; }

    public EfAlterColumnFixture(bool shrink = true)
    {
        Root = Path.Combine(Path.GetTempPath(), "csharp-doctor-ef-alter-" + Guid.NewGuid().ToString("N"));
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

        MigrationPath = Path.Combine(appDir, "Migrations", "20260101000000_ShrinkName.cs");
        Directory.CreateDirectory(Path.GetDirectoryName(MigrationPath)!);

        var oldType = shrink ? "nvarchar(500)" : "nvarchar(100)";
        var newType = shrink ? "nvarchar(100)" : "nvarchar(500)";

        File.WriteAllText(
            MigrationPath,
            $$"""
            using Microsoft.EntityFrameworkCore.Migrations;

            namespace App.Migrations;

            public partial class ShrinkName : Migration
            {
                protected override void Up(MigrationBuilder migrationBuilder)
                {
                    migrationBuilder.AlterColumn<string>(
                        name: "Name",
                        table: "Users",
                        type: "{{newType}}",
                        nullable: false,
                        oldClrType: typeof(string),
                        oldType: "{{oldType}}");
                }

                protected override void Down(MigrationBuilder migrationBuilder) { }
            }
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
