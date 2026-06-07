using CSharpDoctor.Analyzers.Rules.Security;
using Xunit;

namespace CSharpDoctor.Analyzers.UnitTests.Rules.Security;

public sealed class Csd0009Tests
{
    [Fact]
    public async Task ShouldReport_WhenDtdProcessingParse()
    {
        await AnalyzerTest<Csd0009UnsafeXmlDtdAnalyzer>.RunAsync(
            """
            class C {
                void M() {
                    var settings = new System.Xml.XmlReaderSettings { DtdProcessing = System.Xml.DtdProcessing.Parse };
                }
            }
            """,
            "CSD0009");
    }

    [Fact]
    public async Task ShouldNotReport_WhenDtdProcessingProhibit()
    {
        await AnalyzerTest<Csd0009UnsafeXmlDtdAnalyzer>.RunAsync(
            """
            class C {
                void M() {
                    var settings = new System.Xml.XmlReaderSettings { DtdProcessing = System.Xml.DtdProcessing.Prohibit };
                }
            }
            """);
    }

    [Fact]
    public async Task ShouldNotReport_WhenPatternOnlyInStringLiteral()
    {
        await AnalyzerTest<Csd0009UnsafeXmlDtdAnalyzer>.RunAsync(
            """
            class C {
                string M() => "DtdProcessing.Parse";
            }
            """);
    }
}
