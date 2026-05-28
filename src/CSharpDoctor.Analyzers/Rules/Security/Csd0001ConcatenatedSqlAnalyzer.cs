using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpDoctor.Analyzers.Rules.Security;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Csd0001ConcatenatedSqlAnalyzer : CSharpDoctorAnalyzerBase
{
    protected override RuleDescriptor Descriptor { get; } = new RuleDescriptor(
        "CSD0001",
        "Concatenated SQL command text",
        "Security",
        DiagnosticSeverity.Warning,
        "SQL command text is built via string concatenation: {0}",
        "Building SQL via concatenation risks SQL injection. Use parameterized commands.",
        new[] { "security" });

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeAddExpression, SyntaxKind.AddExpression);
    }

    private void AnalyzeAddExpression(SyntaxNodeAnalysisContext context)
    {
        if (context.Node.Parent is not AssignmentExpressionSyntax assignment)
        {
            return;
        }

        if (!IsCommandTextAssignment(assignment.Left))
        {
            return;
        }

        Report(context, assignment.GetLocation(), assignment.Left.ToString());
    }

    private static bool IsCommandTextAssignment(ExpressionSyntax left)
    {
        while (left is ParenthesizedExpressionSyntax parentheses)
        {
            left = parentheses.Expression;
        }

        return left is MemberAccessExpressionSyntax { Name.Identifier.Text: "CommandText" };
    }
}
