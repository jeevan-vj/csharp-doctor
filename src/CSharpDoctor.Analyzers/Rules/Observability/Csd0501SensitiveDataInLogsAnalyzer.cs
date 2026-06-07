using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpDoctor.Analyzers.Rules.Observability;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Csd0501SensitiveDataInLogsAnalyzer : CSharpDoctorAnalyzerBase
{
    protected override RuleDescriptor Descriptor { get; } = new RuleDescriptor(
        "CSD0501",
        "Sensitive data in logs",
        "Observability",
        DiagnosticSeverity.Warning,
        "Logging may expose sensitive data: {0}",
        "Remove secrets from log messages and structured properties; log redacted identifiers instead.",
        new[] { "security", "observability" });

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

        if (!IsLoggerCall(invocation))
        {
            return;
        }

        foreach (var argument in invocation.ArgumentList.Arguments)
        {
            var text = argument.Expression.ToString();
            if (LoggingSensitiveNameMatcher.IsSensitiveName(text))
            {
                if (LoggingSensitiveNameMatcher.IsEventNameOnly(text))
                {
                    continue;
                }

                Report(context, argument.GetLocation(), text);
                return;
            }
        }

        var messageArgument = FindMessageTemplateArgument(invocation);
        if (messageArgument is not null
            && LoggingSensitiveNameMatcher.IsSensitiveName(messageArgument.ToString()))
        {
            Report(context, messageArgument.GetLocation(), messageArgument.ToString());
        }
    }

    private static bool IsLoggerCall(InvocationExpressionSyntax invocation)
    {
        var text = invocation.Expression.ToString();
        return text.Contains("Log", StringComparison.OrdinalIgnoreCase)
            && (text.Contains("logger", StringComparison.OrdinalIgnoreCase)
                || text.StartsWith("Log.", StringComparison.Ordinal));
    }

    private static ExpressionSyntax? FindMessageTemplateArgument(InvocationExpressionSyntax invocation)
    {
        foreach (var argument in invocation.ArgumentList.Arguments)
        {
            if (argument.Expression is LiteralExpressionSyntax literal
                && literal.IsKind(SyntaxKind.StringLiteralExpression))
            {
                return argument.Expression;
            }
        }

        return invocation.ArgumentList.Arguments.FirstOrDefault()?.Expression;
    }
}
