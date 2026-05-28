using CSharpDoctor.Analyzers.Rules.Correctness;
using Xunit;

namespace CSharpDoctor.Analyzers.UnitTests.Rules.Correctness;

public sealed class Csd0106Tests
{
    [Fact]
    public async Task ShouldReport_WhenDateTimeNowAssignedToCreatedAt()
    {
        await AnalyzerTest<Csd0106DateTimeNowPersistenceAnalyzer>.RunAsync(
            """
            class Entity {
                public System.DateTime CreatedAt { get; set; }
                public void Stamp() { CreatedAt = System.DateTime.Now; }
            }
            """,
            "CSD0106");
    }

    [Fact]
    public async Task ShouldNotReport_WhenDateTimeUtcNowUsed()
    {
        await AnalyzerTest<Csd0106DateTimeNowPersistenceAnalyzer>.RunAsync(
            """
            class Entity {
                public System.DateTime CreatedAt { get; set; }
                public void Stamp() { CreatedAt = System.DateTime.UtcNow; }
            }
            """);
    }
}
