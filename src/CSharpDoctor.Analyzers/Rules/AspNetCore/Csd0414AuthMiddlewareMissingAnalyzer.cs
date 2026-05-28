using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpDoctor.Analyzers.Rules.AspNetCore;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Csd0414AuthMiddlewareMissingAnalyzer : CSharpDoctorAnalyzerBase
{
    protected override RuleDescriptor Descriptor { get; } = new RuleDescriptor(
        "CSD0414",
        "Authentication middleware missing",
        "AspNetCore",
        DiagnosticSeverity.Warning,
        "AddAuthentication without UseAuthentication in pipeline",
        "Call app.UseAuthentication() after building the host when authentication services are registered.",
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
        if (!text.Contains("AddAuthentication", StringComparison.Ordinal))
        {
            return;
        }

        if (text.Contains("UseAuthentication", StringComparison.Ordinal))
        {
            return;
        }

        Report(context, context.Node.GetLocation());
    }
}
