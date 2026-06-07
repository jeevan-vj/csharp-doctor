using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpDoctor.Analyzers.Rules.Performance;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Csde0008SaveChangesInLoopAnalyzer : CSharpDoctorAnalyzerBase
{
    protected override RuleDescriptor Descriptor { get; } = new RuleDescriptor(
        "CSDE0008",
        "SaveChanges called inside loop",
        "Performance",
        DiagnosticSeverity.Warning,
        "SaveChanges inside a loop causes N round-trips to the database",
        "Batch changes and call SaveChanges once outside the loop.",
        new[] { "efcore", "performance" });

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.InvocationExpression);
    }

    private void Analyze(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not InvocationExpressionSyntax invocation
            || invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
        {
            return;
        }

        var name = memberAccess.Name.Identifier.ValueText;
        if (name is not ("SaveChanges" or "SaveChangesAsync"))
        {
            return;
        }

        if (!IsInsideLoop(invocation))
        {
            return;
        }

        Report(context, invocation.GetLocation());
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
