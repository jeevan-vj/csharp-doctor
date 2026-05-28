using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpDoctor.Analyzers.Rules.Performance;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Csd0202StringConcatInLoopAnalyzer : CSharpDoctorAnalyzerBase
{
    protected override RuleDescriptor Descriptor { get; } = new RuleDescriptor(
        "CSD0202",
        "String concatenation in loop",
        "Performance",
        DiagnosticSeverity.Warning,
        "String concatenation in loop; prefer StringBuilder",
        "Repeated string concatenation in loops allocates heavily. Use StringBuilder or string.Create.",
        new[] { "performance" });

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.AddAssignmentExpression, SyntaxKind.SimpleAssignmentExpression);
    }

    private void Analyze(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not AssignmentExpressionSyntax assignment)
        {
            return;
        }

        if (!IsStringConcat(assignment))
        {
            return;
        }

        if (!IsInsideLoop(assignment))
        {
            return;
        }

        Report(context, assignment.GetLocation());
    }

    private static bool IsStringConcat(AssignmentExpressionSyntax assignment) =>
        assignment.IsKind(SyntaxKind.AddAssignmentExpression)
        || (assignment.Right is BinaryExpressionSyntax right && right.IsKind(SyntaxKind.AddExpression));

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
