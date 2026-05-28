using CSharpDoctor.Core.Suppression;
using Shouldly;
using Xunit;

namespace CSharpDoctor.Core.UnitTests.Suppression;

public sealed class SuppressionEvaluatorTests
{
    [Fact]
    public void InlineDisableComment_RemovesDiagnostic()
    {
        var lines = new[] { "// csharp-doctor-disable-next-line", "var x = 1;" };
        SuppressionEvaluator.Evaluate(lines, 2, "csharp-doctor/CSD0001")
            .IsSuppressed.ShouldBeTrue();
    }

    [Fact]
    public void NearMissHint_WhenCommentAlmostMatches()
    {
        var lines = new[] { "// csharp-doctor-disable CSD0001", "var x = 1;" };
        var result = SuppressionEvaluator.Evaluate(lines, 2, "csharp-doctor/CSD0001");
        result.IsSuppressed.ShouldBeFalse();
        result.NearMissHint.ShouldNotBeNullOrWhiteSpace();
    }
}
