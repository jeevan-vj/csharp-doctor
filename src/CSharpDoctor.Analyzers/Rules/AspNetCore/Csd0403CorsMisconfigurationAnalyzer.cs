using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpDoctor.Analyzers.Rules.AspNetCore;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Csd0403CorsMisconfigurationAnalyzer : CSharpDoctorAnalyzerBase
{
    protected override RuleDescriptor Descriptor { get; } = new RuleDescriptor(
        "CSD0403",
        "Insecure CORS configuration",
        "AspNetCore",
        DiagnosticSeverity.Error,
        "AllowAnyOrigin combined with AllowCredentials is invalid",
        "Do not call AllowAnyOrigin() with AllowCredentials(). Use explicit origins instead.",
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
        if (text.Contains("AllowAnyOrigin", StringComparison.Ordinal)
            && text.Contains("AllowCredentials", StringComparison.Ordinal))
        {
            Report(context, context.Node.GetLocation());
        }
    }
}
