using CSharpDoctor.Core.Diagnostics;
using Shouldly;
using Xunit;

namespace CSharpDoctor.Core.UnitTests.Diagnostics;

public sealed class DiagnosticIdentityTests
{
    [Fact]
    public void StableIdentity_IsFilePathLineColRuleId()
    {
        var diagnostic = new DiagnosticRecord(
            FilePath: @"src\Foo.cs",
            Plugin: "csharp-doctor",
            Rule: "CSD0001",
            Severity: DiagnosticSeverity.Warning,
            Message: "msg",
            Help: "help",
            Line: 10,
            Column: 4,
            Category: "Security",
            Tags: []);

        DiagnosticIdentity.Build(diagnostic)
            .ShouldBe("src/Foo.cs::10:4::csharp-doctor/CSD0001");
    }
}
