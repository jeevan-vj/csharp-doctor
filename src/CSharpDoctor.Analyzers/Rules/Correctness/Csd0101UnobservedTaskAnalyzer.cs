using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpDoctor.Analyzers.Rules.Correctness;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Csd0101UnobservedTaskAnalyzer : CSharpDoctorAnalyzerBase
{
    protected override RuleDescriptor Descriptor { get; } = new RuleDescriptor(
        "CSD0101",
        "Unobserved task",
        "Correctness",
        DiagnosticSeverity.Warning,
        "Task-returning call is not awaited or assigned: {0}",
        "Await the task, assign to a variable and handle errors, or explicitly discard with error handling.",
        ["correctness", "async"]);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.ExpressionStatement);
    }

    private void Analyze(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not ExpressionStatementSyntax statement
            || statement.Expression is not InvocationExpressionSyntax invocation)
        {
            return;
        }

        var symbol = context.SemanticModel.GetSymbolInfo(invocation).Symbol;
        if (symbol is not IMethodSymbol method
            || !IsTaskLike(method.ReturnType))
        {
            return;
        }

        if (statement.Expression is AssignmentExpressionSyntax)
        {
            return;
        }

        Report(context, invocation.GetLocation(), invocation.Expression.ToString());
    }

    private static bool IsTaskLike(ITypeSymbol type) =>
        type.Name is "Task" or "ValueTask"
        || type.BaseType?.Name is "Task";
}
