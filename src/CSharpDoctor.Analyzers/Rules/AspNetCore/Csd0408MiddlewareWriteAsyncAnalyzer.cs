using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpDoctor.Analyzers.Rules.AspNetCore;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Csd0408MiddlewareWriteAsyncAnalyzer : CSharpDoctorAnalyzerBase
{
    protected override RuleDescriptor Descriptor { get; } = new RuleDescriptor(
        "CSD0408",
        "Response.WriteAsync without try/finally in middleware",
        "AspNetCore",
        DiagnosticSeverity.Warning,
        "WriteAsync in middleware should use try/finally",
        "Wrap Response.WriteAsync calls in try/finally to avoid leaving the response in a bad state on failure.",
        new[] { "aspnet", "correctness" });

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.InvocationExpression);
    }

    private void Analyze(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not InvocationExpressionSyntax invocation
            || invocation.Expression is not MemberAccessExpressionSyntax member
            || member.Name.Identifier.Text != "WriteAsync")
        {
            return;
        }

        if (!IsInMiddlewareContext(invocation))
        {
            return;
        }

        if (IsInsideTryFinally(invocation))
        {
            return;
        }

        Report(context, invocation.GetLocation());
    }

    private static bool IsInMiddlewareContext(SyntaxNode node)
    {
        for (var current = node.Parent; current is not null; current = current.Parent)
        {
            var text = current.ToString();
            if (text.Contains(".Use(", StringComparison.Ordinal)
                || text.Contains("IMiddleware", StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsInsideTryFinally(SyntaxNode node)
    {
        for (var current = node.Parent; current is not null; current = current.Parent)
        {
            if (current is TryStatementSyntax tryStatement
                && tryStatement.Contains(node))
            {
                return true;
            }
        }

        return false;
    }
}
