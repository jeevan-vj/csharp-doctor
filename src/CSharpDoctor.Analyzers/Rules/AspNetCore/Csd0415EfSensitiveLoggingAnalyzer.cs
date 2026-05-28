using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpDoctor.Analyzers.Rules.AspNetCore;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Csd0415EfSensitiveLoggingAnalyzer : CSharpDoctorAnalyzerBase
{
    protected override RuleDescriptor Descriptor { get; } = new RuleDescriptor(
        "CSD0415",
        "EF sensitive data logging enabled",
        "AspNetCore",
        DiagnosticSeverity.Warning,
        "EnableSensitiveDataLogging is enabled",
        "Disable sensitive data logging in production; use structured logging with redaction.",
        new[] { "aspnet", "security" });

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.InvocationExpression);
    }

    private void Analyze(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not InvocationExpressionSyntax invocation)
        {
            return;
        }

        var text = invocation.ToString();
        if (text.Contains("EnableSensitiveDataLogging", StringComparison.Ordinal))
        {
            Report(context, invocation.GetLocation());
        }
    }
}
