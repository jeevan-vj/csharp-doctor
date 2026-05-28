using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpDoctor.Analyzers.Rules.AspNetCore;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Csd0405NewtonsoftJsonAnalyzer : CSharpDoctorAnalyzerBase
{
    protected override RuleDescriptor Descriptor { get; } = new RuleDescriptor(
        "CSD0405",
        "Newtonsoft.Json without version pin",
        "AspNetCore",
        DiagnosticSeverity.Warning,
        "AddNewtonsoftJson() should pin a package version in the project file",
        "Pin Microsoft.AspNetCore.Mvc.NewtonsoftJson to a supported version to reduce supply-chain risk.",
        new[] { "aspnet", "security" });

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.InvocationExpression);
    }

    private void Analyze(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not InvocationExpressionSyntax invocation
            || !invocation.ToString().Contains("AddNewtonsoftJson", StringComparison.Ordinal))
        {
            return;
        }

        Report(context, invocation.GetLocation());
    }
}
