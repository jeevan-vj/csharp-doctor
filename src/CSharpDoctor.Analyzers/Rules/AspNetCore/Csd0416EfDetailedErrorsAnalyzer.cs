using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpDoctor.Analyzers.Rules.AspNetCore;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Csd0416EfDetailedErrorsAnalyzer : CSharpDoctorAnalyzerBase
{
    protected override RuleDescriptor Descriptor { get; } = new RuleDescriptor(
        "CSD0416",
        "EF detailed errors enabled",
        "AspNetCore",
        DiagnosticSeverity.Warning,
        "EnableDetailedErrors is enabled",
        "Disable detailed EF errors in production to avoid leaking schema and query details.",
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
        if (text.Contains("EnableDetailedErrors", StringComparison.Ordinal))
        {
            Report(context, invocation.GetLocation());
        }
    }
}
