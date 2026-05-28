using CSharpDoctor.Core.Configuration;
using CSharpDoctor.Core.Diagnostics;
using CSharpDoctor.Core.Pipeline;
using Shouldly;
using Xunit;

namespace CSharpDoctor.Core.UnitTests.Pipeline;

public sealed class DiagnosticPipelineTests
{
    private static DiagnosticRecord Sample() =>
        new(
            FilePath: "src/App.cs",
            Plugin: "csharp-doctor",
            Rule: "CSD0001",
            Severity: DiagnosticSeverity.Warning,
            Message: "m",
            Help: "h",
            Line: 1,
            Column: 1,
            Category: "Security",
            Tags: ["design"]);

    [Fact]
    public void AppliesTransformsInOrder_SeverityThenIgnoreThenSuppression()
    {
        var config = new CSharpDoctorConfig
        {
            Rules = new Dictionary<string, SeverityOverride>
            {
                ["csharp-doctor/CSD0001"] = SeverityOverride.Off,
            },
        };

        var pipeline = new DiagnosticPipeline([new SeverityOverrideTransform(config)]);
        pipeline.Apply([Sample()]).ShouldBeEmpty();
    }

    [Fact]
    public void IgnoreTransform_DropsByTag()
    {
        var config = new CSharpDoctorConfig
        {
            Ignore = new IgnoreConfig { Tags = ["design"] },
        };

        var pipeline = new DiagnosticPipeline([new IgnoreTransform(config)]);
        pipeline.Apply([Sample()]).ShouldBeEmpty();
    }
}
