using CSharpDoctor.Cli;
using CSharpDoctor.Cli.IntegrationTests.Fixtures;
using Shouldly;
using Xunit;

namespace CSharpDoctor.Cli.IntegrationTests;

public sealed class HtmlOutputTests
{
    [Fact]
    public async Task Run_FormatHtmlWithoutOutput_WritesDefaultReportFile()
    {
        using var fixture = new TempSolutionFixture();
        var defaultReportPath = Path.Combine(fixture.Root, "csharp-doctor-report.html");
        var output = new StringWriter();
        var error = new StringWriter();

        var exitCode = await CliRunner.RunAsync(
            ["--format", "html", fixture.Root],
            output,
            error);

        File.Exists(defaultReportPath).ShouldBeTrue();
        var html = await File.ReadAllTextAsync(defaultReportPath);
        html.ShouldContain("<!DOCTYPE html>");
        html.ShouldContain("id=\"summary\"");
        output.ToString().ShouldNotContain("<!DOCTYPE html>");
        output.ToString().ShouldContain(defaultReportPath);
    }

    [Fact]
    public async Task Run_FormatHtmlWithOutput_WritesFileAndLeavesStdoutMinimal()
    {
        using var fixture = new TempSolutionFixture();
        var reportPath = Path.Combine(fixture.Root, "report.html");
        var output = new StringWriter();
        var error = new StringWriter();

        var exitCode = await CliRunner.RunAsync(
            ["--format", "html", "--output", reportPath, fixture.Root],
            output,
            error);

        File.Exists(reportPath).ShouldBeTrue();
        var html = await File.ReadAllTextAsync(reportPath);
        html.ShouldContain("id=\"summary\"");
        html.ShouldContain("\"schemaVersion\": 2");
        output.ToString().ShouldNotContain("<!DOCTYPE html>");
        output.ToString().ShouldContain(reportPath);
    }
}
