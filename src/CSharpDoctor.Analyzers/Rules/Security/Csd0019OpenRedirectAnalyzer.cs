using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpDoctor.Analyzers.Rules.Security;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Csd0019OpenRedirectAnalyzer : CSharpDoctorAnalyzerBase
{
    protected override RuleDescriptor Descriptor { get; } = new RuleDescriptor(
        "CSD0019",
        "Open redirect via dynamic URL",
        "Security",
        DiagnosticSeverity.Warning,
        "Redirect uses a dynamic URL: {0}",
        "Use LocalRedirect or validate URLs with Url.IsLocalUrl before redirecting.",
        new[] { "security", "aspnet" });

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

        var isRedirect = invocation.Expression switch
        {
            MemberAccessExpressionSyntax member when SecuritySinkHelpers.IsRedirectSink(member) => true,
            IdentifierNameSyntax { Identifier.Text: "Redirect" or "RedirectPermanent" } => true,
            _ => SecuritySinkHelpers.IsResultsRedirect(invocation),
        };

        if (!isRedirect)
        {
            return;
        }

        var urlArgument = invocation.ArgumentList.Arguments.FirstOrDefault()?.Expression;
        if (urlArgument is null || !SecuritySinkHelpers.IsDynamicExpression(urlArgument))
        {
            return;
        }

        if (SecuritySinkHelpers.IsInsideMethod(invocation, out var method)
            && SecuritySinkHelpers.MethodContainsText(method!, "IsLocalUrl", "LocalRedirect"))
        {
            return;
        }

        Report(context, urlArgument.GetLocation(), urlArgument.ToString());
    }
}
