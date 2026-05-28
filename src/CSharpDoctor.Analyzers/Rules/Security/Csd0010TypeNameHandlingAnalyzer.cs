using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpDoctor.Analyzers.Rules.Security;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Csd0010TypeNameHandlingAnalyzer : CSharpDoctorAnalyzerBase
{
    protected override RuleDescriptor Descriptor { get; } = new RuleDescriptor(
        "CSD0010",
        "Unsafe Newtonsoft TypeNameHandling",
        "Security",
        DiagnosticSeverity.Error,
        "TypeNameHandling is not None: {0}",
        "Set TypeNameHandling to None to prevent remote code execution via polymorphic deserialization.",
        new[] { "security" });

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.CompilationUnit);
    }

    private void Analyze(SyntaxNodeAnalysisContext context)
    {
        var text = context.Node.ToString();
        if (!text.Contains("TypeNameHandling", StringComparison.Ordinal))
        {
            return;
        }

        if (text.Contains("TypeNameHandling.None", StringComparison.Ordinal)
            && !text.Contains("TypeNameHandling.All", StringComparison.Ordinal)
            && !text.Contains("TypeNameHandling.Auto", StringComparison.Ordinal)
            && !text.Contains("TypeNameHandling.Objects", StringComparison.Ordinal)
            && !text.Contains("TypeNameHandling.Arrays", StringComparison.Ordinal))
        {
            return;
        }

        if (text.Contains("TypeNameHandling.All", StringComparison.Ordinal)
            || text.Contains("TypeNameHandling.Auto", StringComparison.Ordinal)
            || text.Contains("TypeNameHandling.Objects", StringComparison.Ordinal)
            || text.Contains("TypeNameHandling.Arrays", StringComparison.Ordinal)
            || text.Contains("TypeNameHandling = 1", StringComparison.Ordinal)
            || text.Contains("TypeNameHandling = 2", StringComparison.Ordinal)
            || text.Contains("TypeNameHandling = 3", StringComparison.Ordinal)
            || text.Contains("TypeNameHandling = 4", StringComparison.Ordinal))
        {
            Report(context, context.Node.GetLocation(), "TypeNameHandling");
        }
    }
}
