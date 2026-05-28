using CSharpDoctor.Analyzers.Rules.AspNetCore;
using Xunit;

namespace CSharpDoctor.Analyzers.UnitTests.Rules.AspNetCore;

public sealed class Csd0404Tests
{
    [Fact]
    public async Task ShouldReport_WhenIFormFileWithoutSizeLimit()
    {
        await AnalyzerTest<Csd0404FormFileSizeLimitAnalyzer>.RunAsync(
            """
            interface IFormFile { }
            class UploadController {
                void Post(IFormFile file) { }
            }
            """,
            "CSD0404");
    }

    [Fact]
    public async Task ShouldNotReport_WhenRequestSizeLimitConfigured()
    {
        await AnalyzerTest<Csd0404FormFileSizeLimitAnalyzer>.RunAsync(
            """
            interface IFormFile { }
            class UploadController {
                [RequestSizeLimit(1024)]
                void Post(IFormFile file) { }
            }
            class RequestSizeLimitAttribute : System.Attribute {
                public RequestSizeLimitAttribute(long bytes) { }
            }
            """);
    }
}
