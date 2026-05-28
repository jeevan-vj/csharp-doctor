using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpDoctor.Analyzers.Rules.Security;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Csd0011HttpClientCertBypassAnalyzer : CSharpDoctorAnalyzerBase
{
    protected override RuleDescriptor Descriptor { get; } = new RuleDescriptor(
        "CSD0011",
        "TLS certificate validation bypass",
        "Security",
        DiagnosticSeverity.Error,
        "ServerCertificateCustomValidationCallback always returns true",
        "Do not disable TLS certificate validation in production. Use proper trust chains or development-only guards.",
        new[] { "security" });

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.SimpleLambdaExpression);
        context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.ParenthesizedLambdaExpression);
        context.RegisterSyntaxNodeAction(AnalyzeAssignment, SyntaxKind.SimpleAssignmentExpression);
    }

    private void Analyze(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not LambdaExpressionSyntax lambda)
        {
            return;
        }

        if (!IsCertValidationCallbackAssignment(lambda.Parent))
        {
            return;
        }

        if (LambdaAlwaysReturnsTrue(lambda))
        {
            Report(context, lambda.GetLocation());
        }
    }

    private void AnalyzeAssignment(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not AssignmentExpressionSyntax assignment)
        {
            return;
        }

        if (!assignment.Left.ToString().Contains("ServerCertificateCustomValidationCallback", StringComparison.Ordinal))
        {
            return;
        }

        if (assignment.Right is LambdaExpressionSyntax lambda && LambdaAlwaysReturnsTrue(lambda))
        {
            Report(context, assignment.GetLocation());
        }
        else if (assignment.Right.ToString().Contains("=> true", StringComparison.Ordinal)
                 || assignment.Right.ToString().Contains("return true", StringComparison.Ordinal))
        {
            Report(context, assignment.GetLocation());
        }
    }

    private static bool IsCertValidationCallbackAssignment(SyntaxNode? parent) =>
        parent is AssignmentExpressionSyntax assignment
        && assignment.Left.ToString().Contains("ServerCertificateCustomValidationCallback", StringComparison.Ordinal);

    private static bool LambdaAlwaysReturnsTrue(LambdaExpressionSyntax lambda)
    {
        var body = lambda.Body;
        if (body is LiteralExpressionSyntax { RawKind: (int)SyntaxKind.TrueLiteralExpression })
        {
            return true;
        }

        if (body is BlockSyntax block)
        {
            var returns = block.DescendantNodes().OfType<ReturnStatementSyntax>().ToList();
            return returns.Count > 0
                && returns.All(r => r.Expression is LiteralExpressionSyntax { RawKind: (int)SyntaxKind.TrueLiteralExpression });
        }

        return body.ToString().Trim() == "true";
    }
}
