using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpDoctor.Analyzers.Rules.Security;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Csd0018SsrfAnalyzer : CSharpDoctorAnalyzerBase
{
    protected override RuleDescriptor Descriptor { get; } = new RuleDescriptor(
        "CSD0018",
        "SSRF via dynamic HTTP request URL",
        "Security",
        DiagnosticSeverity.Warning,
        "HTTP client call uses a dynamic URL: {0}",
        "Validate outbound URLs against an allowlist before calling external services.",
        new[] { "security", "aspnet" });

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
        context.RegisterSyntaxNodeAction(AnalyzeObjectCreation, SyntaxKind.ObjectCreationExpression);
    }

    private void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not InvocationExpressionSyntax invocation
            || invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
        {
            return;
        }

        if (!SecuritySinkHelpers.IsHttpClientSink(memberAccess))
        {
            return;
        }

        var urlArgument = invocation.ArgumentList.Arguments.FirstOrDefault()?.Expression;
        if (urlArgument is null || !SecuritySinkHelpers.IsDynamicExpression(urlArgument))
        {
            return;
        }

        if (HasAllowlistGuard(invocation))
        {
            return;
        }

        Report(context, urlArgument.GetLocation(), memberAccess.Name.Identifier.Text);
    }

    private void AnalyzeObjectCreation(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not ObjectCreationExpressionSyntax creation
            || !creation.Type.ToString().Contains("Uri", StringComparison.Ordinal))
        {
            return;
        }

        var uriArgument = creation.ArgumentList?.Arguments.FirstOrDefault()?.Expression;
        if (uriArgument is null || !SecuritySinkHelpers.IsDynamicExpression(uriArgument))
        {
            return;
        }

        if (HasAllowlistGuard(creation))
        {
            return;
        }

        Report(context, uriArgument.GetLocation(), "Uri");
    }

    private static bool HasAllowlistGuard(SyntaxNode node)
    {
        if (!SecuritySinkHelpers.IsInsideMethod(node, out var method))
        {
            return false;
        }

        var text = method!.ToString();
        return text.Contains("allowedHosts", StringComparison.OrdinalIgnoreCase)
            || text.Contains("IsLocalUrl", StringComparison.Ordinal)
            || text.Contains(".Host", StringComparison.Ordinal) && text.Contains("Contains", StringComparison.Ordinal);
    }
}
