using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpDoctor.Analyzers.Rules.Performance;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Csd0201LinqMaterializationInLoopAnalyzer : CSharpDoctorAnalyzerBase
{
    private static readonly string[] Materializers = ["ToList", "ToArray", "ToDictionary"];

    protected override RuleDescriptor Descriptor { get; } = new RuleDescriptor(
        "CSD0201",
        "LINQ materialization in loop",
        "Performance",
        DiagnosticSeverity.Warning,
        "LINQ materialization '{0}' inside loop causes repeated allocations",
        "Move materialization outside the loop or use incremental processing.",
        new[] { "performance" });

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.InvocationExpression);
    }

    private void Analyze(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not InvocationExpressionSyntax invocation
            || invocation.Expression is not MemberAccessExpressionSyntax member)
        {
            return;
        }

        if (!Materializers.Contains(member.Name.Identifier.Text))
        {
            return;
        }

        if (!IsInsideLoop(invocation))
        {
            return;
        }

        Report(context, invocation.GetLocation(), member.Name.Identifier.Text);
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
