using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpDoctor.Analyzers.Rules.Performance;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Csd0204HttpClientInLoopAnalyzer : CSharpDoctorAnalyzerBase
{
    protected override RuleDescriptor Descriptor { get; } = new RuleDescriptor(
        "CSD0204",
        "HttpClient created in loop",
        "Performance",
        DiagnosticSeverity.Warning,
        "Creating HttpClient inside a loop can exhaust sockets",
        "Reuse a single HttpClient or IHttpClientFactory for the application lifetime.",
        new[] { "performance" });

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.ObjectCreationExpression, SyntaxKind.UsingStatement);
    }

    private void Analyze(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is ObjectCreationExpressionSyntax creation
            && creation.Type.ToString().Contains("HttpClient"))
        {
            if (IsInsideLoop(creation))
            {
                Report(context, creation.GetLocation());
            }
        }
        else if (context.Node is UsingStatementSyntax usingStatement
            && usingStatement.Declaration?.ToString().Contains("HttpClient") == true
            && IsInsideLoop(usingStatement))
        {
            Report(context, usingStatement.GetLocation());
        }
    }

    private static bool IsInsideLoop(SyntaxNode node)
    {
        for (var current = node.Parent; current is not null; current = current.Parent)
        {
            if (current is ForStatementSyntax or ForEachStatementSyntax or WhileStatementSyntax or DoStatementSyntax)
            {
                return true;
            }
        }

        return false;
    }
}
