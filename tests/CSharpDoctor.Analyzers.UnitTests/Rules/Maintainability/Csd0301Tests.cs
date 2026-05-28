using CSharpDoctor.Analyzers.Rules.Maintainability;
using Xunit;

namespace CSharpDoctor.Analyzers.UnitTests.Rules.Maintainability;

public sealed class Csd0301Tests
{
    [Fact]
    public async Task ShouldReport_WhenPublicClassHasPublicField()
    {
        await AnalyzerTest<Csd0301PublicFieldAnalyzer>.RunAsync(
            """
            public class Customer {
                public string Name;
            }
            """,
            "CSD0301");
    }

    [Fact]
    public async Task ShouldNotReport_WhenFieldIsPrivate()
    {
        await AnalyzerTest<Csd0301PublicFieldAnalyzer>.RunAsync(
            """
            public class Customer {
                private string _name;
            }
            """);
    }
}
