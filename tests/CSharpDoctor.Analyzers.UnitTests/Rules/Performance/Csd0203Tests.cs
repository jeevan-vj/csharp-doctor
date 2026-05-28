using CSharpDoctor.Analyzers.Rules.Performance;
using Xunit;

namespace CSharpDoctor.Analyzers.UnitTests.Rules.Performance;

public sealed class Csd0203Tests
{
    [Fact]
    public async Task ShouldReport_WhenValueTypePassedToObjectLogParameter()
    {
        await AnalyzerTest<Csd0203BoxingInLoggingAnalyzer>.RunAsync(
            """
            class Logger {
                public void LogInformation(string message, object arg) { }
            }
            class C {
                void M(Logger logger) {
                    logger.LogInformation("Count {0}", 42);
                }
            }
            """,
            "CSD0203");
    }

    [Fact]
    public async Task ShouldNotReport_WhenNoLoggingCall()
    {
        await AnalyzerTest<Csd0203BoxingInLoggingAnalyzer>.RunAsync(
            """
            class C { int M() => 42; }
            """);
    }
}
