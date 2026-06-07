using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpDoctor.Analyzers.Rules.Security;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Csd0009UnsafeXmlDtdAnalyzer : CSharpDoctorAnalyzerBase
{
    protected override RuleDescriptor Descriptor { get; } = new RuleDescriptor(
        "CSD0009",
        "Unsafe XML DTD processing",
        "Security",
        DiagnosticSeverity.Warning,
        "XML reader allows DTD processing: {0}",
        "Set DtdProcessing to Prohibit or Ignore to prevent XML external entity attacks.",
        new[] { "security" });

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.CompilationUnit);
    }

    private void Analyze(SyntaxNodeAnalysisContext context)
    {
        var text = CodeTextExtractor.ToCodeText(context.Node);
        if (text.Contains("DtdProcessing.Parse", StringComparison.Ordinal)
            || text.Contains("DtdProcessing = Parse", StringComparison.Ordinal))
        {
            Report(context, context.Node.GetLocation(), "DtdProcessing.Parse");
            return;
        }

        if (text.Contains("ProhibitDtd = false", StringComparison.Ordinal)
            || text.Contains("ProhibitDtd=false", StringComparison.Ordinal))
        {
            Report(context, context.Node.GetLocation(), "ProhibitDtd = false");
        }
    }
}
