using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpDoctor.Analyzers.Rules.Security;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Csd0013InsecureJwtOptionsAnalyzer : CSharpDoctorAnalyzerBase
{
    protected override RuleDescriptor Descriptor { get; } = new RuleDescriptor(
        "CSD0013",
        "Insecure JWT bearer options",
        "Security",
        DiagnosticSeverity.Warning,
        "JWT option disables security check: {0}",
        "Keep RequireHttpsMetadata and ValidateIssuer enabled in production.",
        new[] { "security", "aspnet" });

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.CompilationUnit);
    }

    private void Analyze(SyntaxNodeAnalysisContext context)
    {
        var text = context.Node.ToString();

        if (text.Contains("RequireHttpsMetadata = false", StringComparison.Ordinal)
            || text.Contains("RequireHttpsMetadata=false", StringComparison.Ordinal))
        {
            Report(context, context.Node.GetLocation(), "RequireHttpsMetadata = false");
        }

        if (text.Contains("ValidateIssuer = false", StringComparison.Ordinal)
            || text.Contains("ValidateIssuer=false", StringComparison.Ordinal))
        {
            Report(context, context.Node.GetLocation(), "ValidateIssuer = false");
        }
    }
}
