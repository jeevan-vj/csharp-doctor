using CSharpDoctor.Core.Diagnostics;
using CSharpDoctor.Core.Inspection;
using CSharpDoctor.Core.Reporting;
using Shouldly;
using Xunit;

namespace CSharpDoctor.Core.UnitTests.Reporting;

public sealed class ConsoleReporterTests
{
    private static readonly DiagnosticRecord Sample = new(
        "a.cs", "csharp-doctor", "CSD0001", DiagnosticSeverity.Warning, "msg", "help", 1, 1, "Security", []);

    [Fact]
    public void Render_WithSingleInspector_OmitsBreakdown()
    {
        var runs = new Dictionary<string, InspectorRunInfo>
        {
            ["lint"] = new(TimeSpan.FromMilliseconds(12), 1, Ran: true),
        };

        var output = ConsoleReporter.Render([Sample], TimeSpan.FromSeconds(1), runs);

        output.ShouldNotContain("Inspectors:");
    }

    [Fact]
    public void Render_WithoutInspectorRuns_MatchesLegacyOutput()
    {
        var legacy = ConsoleReporter.Render([Sample], TimeSpan.FromSeconds(1));
        var withNull = ConsoleReporter.Render([Sample], TimeSpan.FromSeconds(1), inspectorRuns: null);

        withNull.ShouldBe(legacy);
        legacy.ShouldNotContain("Inspectors:");
    }

    [Fact]
    public void Render_WithMultipleInspectors_AppendsBreakdown()
    {
        var runs = new Dictionary<string, InspectorRunInfo>
        {
            ["lint"] = new(TimeSpan.FromMilliseconds(12), 1, Ran: true),
            ["wiring"] = new(TimeSpan.FromMilliseconds(340), 2, Ran: true),
        };

        var output = ConsoleReporter.Render([Sample], TimeSpan.FromSeconds(1), runs);

        output.ShouldContain("Inspectors:");
        output.ShouldContain("lint");
        output.ShouldContain("wiring");
    }
}
