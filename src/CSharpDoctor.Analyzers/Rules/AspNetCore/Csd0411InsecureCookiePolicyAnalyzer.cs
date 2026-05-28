using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpDoctor.Analyzers.Rules.AspNetCore;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Csd0411InsecureCookiePolicyAnalyzer : CSharpDoctorAnalyzerBase
{
    protected override RuleDescriptor Descriptor { get; } = new RuleDescriptor(
        "CSD0411",
        "Insecure cookie policy",
        "AspNetCore",
        DiagnosticSeverity.Warning,
        "Cookie policy disables security: {0}",
        "Set Secure to Always and HttpOnly to true for authentication cookies.",
        new[] { "aspnet", "security" });

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.CompilationUnit);
    }

    private void Analyze(SyntaxNodeAnalysisContext context)
    {
        var text = context.Node.ToString();

        if (text.Contains("CookieSecurePolicy.None", StringComparison.Ordinal)
            || text.Contains("Secure = CookieSecurePolicy.None", StringComparison.Ordinal))
        {
            Report(context, context.Node.GetLocation(), "Secure = None");
        }

        if (text.Contains("HttpOnly = false", StringComparison.Ordinal)
            || text.Contains("HttpOnly=false", StringComparison.Ordinal))
        {
            Report(context, context.Node.GetLocation(), "HttpOnly = false");
        }
    }
}
