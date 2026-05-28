using CSharpDoctor.Analyzers.Rules.Security;
using Xunit;

namespace CSharpDoctor.Analyzers.UnitTests.Rules.Security;

public sealed class Csd0013Tests
{
    [Fact]
    public async Task ShouldReport_WhenRequireHttpsMetadataFalse()
    {
        await AnalyzerTest<Csd0013InsecureJwtOptionsAnalyzer>.RunAsync(
            """
            class Startup {
                void Configure() {
                    var o = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerOptions {
                        RequireHttpsMetadata = false
                    };
                }
            }
            """,
            "CSD0013");
    }

    [Fact]
    public async Task ShouldNotReport_WhenRequireHttpsMetadataTrue()
    {
        await AnalyzerTest<Csd0013InsecureJwtOptionsAnalyzer>.RunAsync(
            """
            class Startup {
                void Configure() {
                    var o = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerOptions {
                        RequireHttpsMetadata = true
                    };
                }
            }
            """);
    }
}
