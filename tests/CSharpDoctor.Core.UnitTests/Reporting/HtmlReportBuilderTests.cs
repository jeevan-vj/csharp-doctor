using CSharpDoctor.Core.Configuration;
using CSharpDoctor.Core.Diagnostics;
using CSharpDoctor.Core.Inspection;
using CSharpDoctor.Core.Reporting;
using Shouldly;
using Xunit;

namespace CSharpDoctor.Core.UnitTests.Reporting;

public sealed class HtmlReportBuilderTests
{
    [Fact]
    public void Build_ContainsRequiredSections()
    {
        var result = SampleResult();

        var html = HtmlReportBuilder.Build(result, InspectMode.Full);

        html.ShouldContain("id=\"summary\"");
        html.ShouldContain("id=\"quality-gates\"");
        html.ShouldContain("id=\"hot-spots\"");
        html.ShouldContain("id=\"findings\"");
        html.ShouldContain("id=\"recommendations\"");
        html.ShouldContain("id=\"raw-json\"");
    }

    [Fact]
    public void Build_EmbedsSchemaVersion2JsonPayload()
    {
        var html = HtmlReportBuilder.Build(SampleResult(), InspectMode.Full);

        html.ShouldContain("\"schemaVersion\": 2");
        html.ShouldContain("id=\"report-data\"");
    }

    [Fact]
    public void Build_FailedGatesAppearBeforePassedGates()
    {
        var result = SampleResult() with
        {
            QualityGates = new QualityGateEvaluation(
                Enabled: true,
                Passed: false,
                Gates:
                [
                    new QualityGateResult("maxWarnings", 0, 3, false),
                    new QualityGateResult("maxErrors", 10, 1, true),
                ]),
        };

        var html = HtmlReportBuilder.Build(result, InspectMode.Full);
        var failIndex = html.IndexOf("maxWarnings", StringComparison.Ordinal);
        var passIndex = html.IndexOf("maxErrors", StringComparison.Ordinal);

        failIndex.ShouldBeGreaterThan(-1);
        passIndex.ShouldBeGreaterThan(-1);
        failIndex.ShouldBeLessThan(passIndex);
    }

    [Fact]
    public void Build_EncodesDangerousDiagnosticText()
    {
        var result = new InspectResult(
            Diagnostics:
            [
                new DiagnosticRecord(
                    "a.cs",
                    "csharp-doctor",
                    "CSD0001",
                    DiagnosticSeverity.Warning,
                    "<script>alert('x')</script>",
                    "Help & \"quotes\"",
                    1,
                    1,
                    "Security",
                    []),
            ],
            ErrorCount: 0,
            WarningCount: 1,
            Elapsed: TimeSpan.FromMilliseconds(5),
            ExitCode: 0,
            Metrics: ScanMetricsCalculator.Compute(
                [
                    new DiagnosticRecord(
                        "a.cs", "csharp-doctor", "CSD0001", DiagnosticSeverity.Warning,
                        "<script>alert('x')</script>", "Help", 1, 1, "Security", []),
                ],
                new DebtWeightsConfig()),
            QualityGates: new QualityGateEvaluation(false, true, []),
            HotSpots: new HotSpotsReport([], [], []));

        var html = HtmlReportBuilder.Build(result, InspectMode.Full);

        html.ShouldNotContain("<script>alert('x')</script>");
        html.ShouldContain("&lt;script&gt;");
    }

    [Fact]
    public void BuildRecommendationGroups_ClassifiesFixNowAndWatch()
    {
        var diagnostics = new[]
        {
            Record(DiagnosticSeverity.Error, "CSD0001"),
            Record(DiagnosticSeverity.Info, "CSDA0003"),
            Record(DiagnosticSeverity.Warning, "CSD0305"),
        };
        var metrics = ScanMetricsCalculator.Compute(diagnostics, new DebtWeightsConfig());
        var groups = HtmlReportViewModel.BuildRecommendationGroups(
            diagnostics,
            metrics,
            new QualityGateEvaluation(true, false, [new QualityGateResult("maxWarnings", 0, 1, false)]),
            checksRan: new HtmlReportChecksRan(true, false, false, false, false),
            configHints: new HtmlReportConfigHints(false, false, false));

        groups.FixNow.ShouldNotBeEmpty();
        groups.Watch.ShouldNotBeEmpty();
        groups.Configure.ShouldNotBeEmpty();
    }

    private static InspectResult SampleResult()
    {
        var diagnostics = new[]
        {
            Record(DiagnosticSeverity.Warning, "CSD0001"),
            Record(DiagnosticSeverity.Error, "CSDS0001"),
        };
        var metrics = ScanMetricsCalculator.Compute(diagnostics, new DebtWeightsConfig());
        var hotSpots = HotSpotRanker.Rank(metrics, new HotSpotsConfig());

        return new InspectResult(
            Diagnostics: diagnostics,
            ErrorCount: 1,
            WarningCount: 1,
            Elapsed: TimeSpan.FromMilliseconds(42),
            ExitCode: 1,
            InspectorRuns: new Dictionary<string, InspectorRunInfo>
            {
                ["lint"] = new(TimeSpan.FromMilliseconds(30), 2, Ran: true),
            },
            Metrics: metrics,
            QualityGates: new QualityGateEvaluation(
                true,
                false,
                [new QualityGateResult("maxErrors", 0, 1, false)]),
            HotSpots: hotSpots);
    }

    private static DiagnosticRecord Record(DiagnosticSeverity severity, string rule) =>
        new("src/App.cs", "csharp-doctor", rule, severity, "message", "help text", 10, 4, "Security", []);
}
