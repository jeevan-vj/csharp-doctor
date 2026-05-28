using CSharpDoctor.Analyzers.Rules.Security;
using Xunit;

namespace CSharpDoctor.Analyzers.UnitTests.Rules.Security;

public sealed class Csd0010Tests
{
    [Fact]
    public async Task ShouldReport_WhenTypeNameHandlingAll()
    {
        await AnalyzerTest<Csd0010TypeNameHandlingAnalyzer>.RunAsync(
            """
            class Startup {
                void Configure() {
                    var s = new Newtonsoft.Json.JsonSerializerSettings {
                        TypeNameHandling = Newtonsoft.Json.TypeNameHandling.All
                    };
                }
            }
            """,
            "CSD0010");
    }

    [Fact]
    public async Task ShouldNotReport_WhenTypeNameHandlingNone()
    {
        await AnalyzerTest<Csd0010TypeNameHandlingAnalyzer>.RunAsync(
            """
            class Startup {
                void Configure() {
                    var s = new Newtonsoft.Json.JsonSerializerSettings {
                        TypeNameHandling = Newtonsoft.Json.TypeNameHandling.None
                    };
                }
            }
            """);
    }
}
