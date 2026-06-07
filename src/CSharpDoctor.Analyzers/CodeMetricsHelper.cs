using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpDoctor.Analyzers;

internal static class CodeMetricsHelper
{
    public static int CountLines(SyntaxNode node) =>
        node.SyntaxTree.GetText().Lines.Count(
            span => span.Start >= node.FullSpan.Start && span.End <= node.FullSpan.End);

    public static int CountCyclomaticComplexity(SyntaxNode node)
    {
        var complexity = 1;
        foreach (var descendant in node.DescendantNodesAndSelf())
        {
            complexity += descendant switch
            {
                IfStatementSyntax => 1,
                WhileStatementSyntax => 1,
                DoStatementSyntax => 1,
                ForStatementSyntax => 1,
                ForEachStatementSyntax => 1,
                CatchClauseSyntax => 1,
                CaseSwitchLabelSyntax => 1,
                ConditionalExpressionSyntax => 1,
                BinaryExpressionSyntax { RawKind: (int)SyntaxKind.LogicalAndExpression or (int)SyntaxKind.LogicalOrExpression } => 1,
                _ => 0,
            };
        }

        return complexity;
    }

    public static int CountLocalVariables(BaseMethodDeclarationSyntax method) =>
        method.DescendantNodes().Count(n => n is VariableDeclaratorSyntax);

    public static int CountInstanceFields(TypeDeclarationSyntax type) =>
        type.Members
            .OfType<FieldDeclarationSyntax>()
            .Where(f => !f.Modifiers.Any(SyntaxKind.StaticKeyword))
            .Sum(f => f.Declaration.Variables.Count);

    public static int CountMethods(TypeDeclarationSyntax type) =>
        type.Members.Count(m => m is MethodDeclarationSyntax or ConstructorDeclarationSyntax);
}
