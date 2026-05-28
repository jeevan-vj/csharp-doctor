using CSharpDoctor.Core.Diagnostics;
using CSharpDoctor.Core.Discovery;
using CSharpDoctor.Core.Pipeline;
using Shouldly;
using Xunit;

namespace CSharpDoctor.Core.UnitTests.Pipeline;

public sealed class CapabilityFilterTransformTests
{
    [Fact]
    public void Apply_SuppressesAspNetRule_WhenProjectLacksAspNetCapability()
    {
        var file = Path.GetFullPath("/tmp/app/Program.cs");
        var transform = new CapabilityFilterTransform(
            new Dictionary<string, ProjectCapabilities>
            {
                [file] = ProjectCapabilities.None,
            });

        var diagnostic = new DiagnosticRecord(
            file, "csharp-doctor", "CSD0401", DiagnosticSeverity.Warning,
            "m", "h", 1, 1, "AspNetCore", ["aspnet"]);

        transform.Apply(diagnostic).ShouldBeNull();
    }

    [Fact]
    public void Apply_KeepsAspNetRule_WhenProjectHasAspNetCapability()
    {
        var file = Path.GetFullPath("/tmp/app/Program.cs");
        var transform = new CapabilityFilterTransform(
            new Dictionary<string, ProjectCapabilities>
            {
                [file] = new ProjectCapabilities(AspNetCore: true, EntityFrameworkCore: false),
            });

        var diagnostic = new DiagnosticRecord(
            file, "csharp-doctor", "CSD0401", DiagnosticSeverity.Warning,
            "m", "h", 1, 1, "AspNetCore", ["aspnet"]);

        transform.Apply(diagnostic).ShouldNotBeNull();
    }
}
