using CSharpDoctor.Core.Configuration;
using CSharpDoctor.Core.Diagnostics;
using CSharpDoctor.Core.Inspection;
using CSharpDoctor.Core.Reporting;
using Shouldly;
using Xunit;

namespace CSharpDoctor.Core.UnitTests.Reporting;

public sealed class JsonReportBuilderTests
{
    [Fact]
    public void Build_IncludesQualityGatesAndHotSpots_WhenProvided()
    {
        var metrics = new ScanMetrics(
            1, 2, 0, 3, 16,
            new Dictionary<string, MetricBucket>(),
            new Dictionary<string, MetricBucket>(),
            new Dictionary<string, MetricBucket>());
        var gates = new QualityGateEvaluation(true, true, []);
        var hotSpots = new HotSpotsReport([], [], []);
        var result = new InspectResult(
            Diagnostics: [],
            ErrorCount: 1,
            WarningCount: 2,
            Elapsed: TimeSpan.FromMilliseconds(5),
            ExitCode: 0,
            Metrics: metrics,
            QualityGates: gates,
            HotSpots: hotSpots);

        var json = JsonReportBuilder.Build(result, InspectMode.Full);

        json.ShouldContain("\"schemaVersion\": 2");
        json.ShouldContain("\"qualityGates\"");
        json.ShouldContain("\"hotSpots\"");
        json.ShouldContain("\"debtScore\": 16");
    }
}
