using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpDoctor.Analyzers.Rules.Performance;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Csd0206StringFormatInLoopAnalyzer : CSharpDoctorAnalyzerBase
{
    protected override RuleDescriptor Descriptor { get; } = new RuleDescriptor(
        "CSD0206",
        "string.Format in loop",
        "Performance",
        DiagnosticSeverity.Warning,
        "string.Format inside a loop causes repeated allocations",
        "Build strings outside the loop or use StringBuilder.",
        new[] { "performance" });

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

        var name = invocation.Expression.ToString();
        if (!name.Contains("string.Format", StringComparison.Ordinal)
            && !name.Contains("String.Format", StringComparison.Ordinal))
        {
            return;
        }

        if (IsInsideLoop(invocation))
        {
            Report(context, invocation.GetLocation());
        }
    }

    private static bool IsInsideLoop(SyntaxNode node)
    {
        for (var current = node.Parent; current is not null; current = current.Parent)
        {
            if (current is ForStatementSyntax or ForEachStatementSyntax or WhileStatementSyntax)
            {
                return true;
            }
        }

        return false;
    }
}
