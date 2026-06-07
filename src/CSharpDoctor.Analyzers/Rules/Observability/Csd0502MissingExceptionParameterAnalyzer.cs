using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpDoctor.Analyzers.Rules.Observability;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Csd0502MissingExceptionParameterAnalyzer : CSharpDoctorAnalyzerBase
{
    protected override RuleDescriptor Descriptor { get; } = new RuleDescriptor(
        "CSD0502",
        "Exception not passed to logger correctly",
        "Observability",
        DiagnosticSeverity.Warning,
        "Exception should be the first parameter to LogError: {0}",
        "Pass the caught exception as the first argument to ILogger.LogError for full stack traces.",
        new[] { "observability" });

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeCatch, SyntaxKind.CatchClause);
    }

    private void AnalyzeCatch(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not CatchClauseSyntax catchClause
            || catchClause.Declaration?.Identifier.ValueText is not string exName)
        {
            return;
        }

        var block = catchClause.Block;
        if (block is null)
        {
            return;
        }

        foreach (var invocation in block.DescendantNodes().OfType<InvocationExpressionSyntax>())
        {
            if (!IsLogError(invocation))
            {
                continue;
            }

            if (UsesExceptionAsTemplateArgument(invocation, exName)
                || LogsOnlyExceptionMessage(invocation, exName))
            {
                Report(context, invocation.GetLocation(), invocation.Expression.ToString());
            }
        }
    }

    private static bool IsLogError(InvocationExpressionSyntax invocation) =>
        invocation.Expression.ToString().Contains("LogError", StringComparison.Ordinal);

    private static bool UsesExceptionAsTemplateArgument(InvocationExpressionSyntax invocation, string exName)
    {
        var args = invocation.ArgumentList.Arguments;
        if (args.Count < 2)
        {
            return false;
        }

        var first = args[0].Expression.ToString();
        if (first == exName)
        {
            return false;
        }

        return args.Skip(1).Any(a => a.Expression.ToString() == exName);
    }

    private static bool LogsOnlyExceptionMessage(InvocationExpressionSyntax invocation, string exName)
    {
        var args = invocation.ArgumentList.Arguments;
        return args.Count == 1
            && args[0].Expression.ToString().Contains($"{exName}.Message", StringComparison.Ordinal);
    }
}
