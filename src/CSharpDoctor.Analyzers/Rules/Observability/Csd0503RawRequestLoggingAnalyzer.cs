using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpDoctor.Analyzers.Rules.Observability;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Csd0503RawRequestLoggingAnalyzer : CSharpDoctorAnalyzerBase
{
    private static readonly string[] RawRequestMembers =
    [
        "Body", "Headers", "Cookies", "Form", "QueryString",
    ];

    protected override RuleDescriptor Descriptor { get; } = new RuleDescriptor(
        "CSD0503",
        "Raw HTTP request data logged",
        "Observability",
        DiagnosticSeverity.Warning,
        "Logging raw HTTP request data: {0}",
        "Avoid logging request bodies, headers, or cookies; redact sensitive request data.",
        new[] { "security", "observability", "aspnet" });

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
            if (ContainsRawRequestAccess(argument.Expression))
            {
                Report(context, argument.GetLocation(), argument.Expression.ToString());
                return;
            }
        }
    }

    private static bool IsLoggerCall(InvocationExpressionSyntax invocation)
    {
        var text = invocation.Expression.ToString();
        return text.Contains("Log", StringComparison.OrdinalIgnoreCase)
            && text.Contains("logger", StringComparison.OrdinalIgnoreCase);
    }

    private static bool ContainsRawRequestAccess(ExpressionSyntax expression) =>
        expression.DescendantNodesAndSelf().OfType<MemberAccessExpressionSyntax>()
            .Any(member =>
            {
                var name = member.Name.Identifier.ValueText;
                if (name.Equals("Authorization", StringComparison.Ordinal)
                    && member.Expression.ToString().Contains("Headers", StringComparison.Ordinal))
                {
                    return true;
                }

                if (!RawRequestMembers.Contains(name, StringComparer.Ordinal))
                {
                    return false;
                }

                var receiver = member.Expression.ToString();
                return receiver.Contains("Request", StringComparison.Ordinal)
                    || receiver.Contains("HttpContext", StringComparison.Ordinal)
                    || receiver.Equals("request", StringComparison.Ordinal);
            });
}
