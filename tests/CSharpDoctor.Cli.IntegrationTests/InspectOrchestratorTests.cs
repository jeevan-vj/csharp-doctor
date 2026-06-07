using CSharpDoctor.Analyzers;
using CSharpDoctor.Core.Configuration;
using CSharpDoctor.Core.Diagnostics;
using CSharpDoctor.Core.Inspection;
using CSharpDoctor.Core.Reporting;
using CSharpDoctor.Cli.IntegrationTests.Fixtures;
using Shouldly;
using Xunit;

namespace CSharpDoctor.Cli.IntegrationTests;

public sealed class InspectOrchestratorTests
{
    [Fact]
    public async Task AspNetScan_ReportsAspNetRules_WhenWebProjectReferenced()
    {
        using var fixture = new AspNetSolutionFixture();
        var orchestrator = new InspectOrchestrator(() => AnalyzerCatalog.CreateAll());

        var result = await orchestrator.RunAsync(
            new InspectRequest(fixture.Root, InspectMode.Full));

        result.Diagnostics.ShouldContain(d => d.Rule == "CSD0402");
        result.Diagnostics.ShouldContain(d => d.Rule == "CSD0403");
    }

    [Fact]
    public async Task NonAspNetScan_SuppressesAspNetRules_OnClassLibraryFixture()
    {
        using var fixture = new TempSolutionFixture();
        File.WriteAllText(
            Path.Combine(Path.GetDirectoryName(fixture.ProjectPath)!, "Program.cs"),
            """
            var builder = WebApplication.CreateBuilder();
            builder.Services.AddSingleton<AppDbContext>();
            app.MapGet("/x", () => "y");
            class AppDbContext { }
            """);

        var orchestrator = new InspectOrchestrator(() => AnalyzerCatalog.CreateAll());
        var result = await orchestrator.RunAsync(
            new InspectRequest(fixture.Root, InspectMode.Full));

        result.Diagnostics.ShouldNotContain(d => d.Rule.StartsWith("CSD04", StringComparison.Ordinal));
    }

    [Fact]
    public async Task FullScan_ReturnsExpectedFindings_ForKnownFixture()
    {
        using var fixture = new TempSolutionFixture();
        var orchestrator = new InspectOrchestrator(() => AnalyzerCatalog.CreateAll());

        var result = await orchestrator.RunAsync(
            new InspectRequest(fixture.Root, InspectMode.Full));

        result.Diagnostics.ShouldNotBeEmpty();
        result.Diagnostics.ShouldContain(d => d.Rule == "CSD0001");
        result.Diagnostics.ShouldContain(d => d.Rule == "CSD0002");
    }

    [Fact]
    public void FailOnError_ExitsNonZero_WhenErrorSeverityPresent()
    {
        var diagnostics = new[]
        {
            new DiagnosticRecord(
                "a.cs", "csharp-doctor", "CSD0003", DiagnosticSeverity.Error,
                "m", "h", 1, 1, "Security", []),
        };

        InspectOrchestrator.ComputeExitCode(diagnostics, FailOnLevel.Error).ShouldBe(1);
    }

    [Fact]
    public async Task QualityGateMaxWarnings_FailsExitCode_WhenWarningsPresent()
    {
        using var fixture = new TempSolutionFixture();
        File.WriteAllText(
            Path.Combine(fixture.Root, ConfigResolver.ConfigFileName),
            """{ "failOn": "none", "qualityGates": { "enabled": true, "maxWarnings": 0 } }""");

        var orchestrator = new InspectOrchestrator(() => AnalyzerCatalog.CreateAll());
        var result = await orchestrator.RunAsync(new InspectRequest(fixture.Root, InspectMode.Full));

        result.ExitCode.ShouldBe(1);
        result.QualityGates!.Passed.ShouldBeFalse();
        result.HotSpots!.IssuesByRule.ShouldNotBeEmpty();
    }

    [Fact]
    public void FailOnNone_AlwaysExitsZero_EvenWithErrors()
    {
        var diagnostics = new[]
        {
            new DiagnosticRecord(
                "a.cs", "csharp-doctor", "CSD0003", DiagnosticSeverity.Error,
                "m", "h", 1, 1, "Security", []),
        };

        InspectOrchestrator.ComputeExitCode(diagnostics, FailOnLevel.None).ShouldBe(0);
    }

    [Fact]
    public void JsonReporter_V1Schema_ContainsSchemaVersion()
    {
        var metrics = new ScanMetrics(
            0, 0, 0, 0, 0,
            new Dictionary<string, MetricBucket>(),
            new Dictionary<string, MetricBucket>(),
            new Dictionary<string, MetricBucket>());
        var result = new InspectResult(
            [], 0, 0, TimeSpan.FromMilliseconds(12), 0,
            Metrics: metrics,
            QualityGates: new QualityGateEvaluation(false, true, []),
            HotSpots: new HotSpotsReport([], [], []));
        var json = JsonReportBuilder.Build(result, InspectMode.Full);
        json.ShouldContain("\"schemaVersion\": 2");
    }

    [Fact]
    public void SarifReporter_EmitsValidSarifVersion()
    {
        var diagnostics = new[]
        {
            new DiagnosticRecord(
                "src/App.cs", "csharp-doctor", "CSD0001", DiagnosticSeverity.Warning,
                "msg", "help", 4, 2, "Security", []),
        };

        var sarif = SarifReportBuilder.Build(diagnostics);
        sarif.ShouldContain("\"version\": \"2.1.0\"");
        sarif.ShouldContain("\"ruleId\": \"CSD0001\"");
    }

    [Fact]
    public void ConsoleReporter_Renders_GroupedByCategory()
    {
        var diagnostics = new[]
        {
            new DiagnosticRecord(
                "a.cs", "csharp-doctor", "CSD0001", DiagnosticSeverity.Warning,
                "msg", "help", 1, 1, "Security", []),
        };

        var output = ConsoleReporter.Render(diagnostics, TimeSpan.FromSeconds(1));
        output.ShouldContain("[Security]");
        output.ShouldContain("CSD0001");
    }
}
