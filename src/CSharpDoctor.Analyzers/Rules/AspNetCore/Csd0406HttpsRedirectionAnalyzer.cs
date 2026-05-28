using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpDoctor.Analyzers.Rules.AspNetCore;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Csd0406HttpsRedirectionAnalyzer : CSharpDoctorAnalyzerBase
{
    protected override RuleDescriptor Descriptor { get; } = new RuleDescriptor(
        "CSD0406",
        "HTTPS redirection not configured",
        "AspNetCore",
        DiagnosticSeverity.Info,
        "Host pipeline does not call UseHttpsRedirection",
        "Call app.UseHttpsRedirection() for production-ready HTTPS enforcement.",
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
        if (!text.Contains("WebApplication", StringComparison.Ordinal)
            && !text.Contains("CreateBuilder", StringComparison.Ordinal))
        {
            return;
        }

        if (text.Contains("UseHttpsRedirection", StringComparison.Ordinal))
        {
            return;
        }

        if (text.Contains("IsDevelopment", StringComparison.Ordinal))
        {
            return;
        }

        Report(context, context.Node.GetLocation());
    }
}
