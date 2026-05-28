using CSharpDoctor.Core.Configuration;
using CSharpDoctor.Core.Diagnostics;
using CSharpDoctor.Core.Pipeline;
using Shouldly;
using Xunit;

namespace CSharpDoctor.Core.UnitTests.Pipeline;

public sealed class SurfaceFilterTests
{
    [Fact]
    public void ExcludesDesignTaggedRules_FromCiFailureAndScore()
    {
        var config = new CSharpDoctorConfig();

        var diagnostic = new DiagnosticRecord(
            "a.cs", "csharp-doctor", "X", DiagnosticSeverity.Warning,
            "m", "h", 1, 1, "Design", ["design"]);

        DiagnosticSurfaceFilter.FilterForSurface([diagnostic], OutputSurface.CiFailure, config)
            .ShouldBeEmpty();
        DiagnosticSurfaceFilter.FilterForSurface([diagnostic], OutputSurface.Score, config)
            .ShouldBeEmpty();
        DiagnosticSurfaceFilter.FilterForSurface([diagnostic], OutputSurface.Cli, config)
            .ShouldHaveSingleItem();
    }
}
