using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpDoctor.Analyzers.Rules.AspNetCore;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Csd0401EndpointAuthAnalyzer : CSharpDoctorAnalyzerBase
{
    private static readonly string[] MapMethods = { "MapGet", "MapPost", "MapPut", "MapDelete", "MapPatch" };

    protected override RuleDescriptor Descriptor { get; } = new RuleDescriptor(
        "CSD0401",
        "Endpoint missing authorization",
        "AspNetCore",
        DiagnosticSeverity.Warning,
        "Minimal API endpoint '{0}' has no authorization",
        "Protect endpoints with RequireAuthorization() or authorization middleware when authentication is configured.",
        new[] { "aspnet", "security" });

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

        var memberName = GetMemberName(invocation.Expression);
        if (!MapMethods.Contains(memberName))
        {
            return;
        }

        if (HasAuthorizationInChain(invocation))
        {
            return;
        }

        var route = invocation.ArgumentList.Arguments.FirstOrDefault()?.ToString() ?? memberName;
        Report(context, invocation.GetLocation(), route);
    }

    private static string GetMemberName(SyntaxNode expression) =>
        expression switch
        {
            MemberAccessExpressionSyntax member => member.Name.Identifier.Text,
            GenericNameSyntax generic => generic.Identifier.Text,
            _ => expression.ToString(),
        };

    private static bool HasAuthorizationInChain(SyntaxNode node)
    {
        for (var current = node; current is not null; current = current.Parent)
        {
            var text = current.ToString();
            if (text.Contains("RequireAuthorization", StringComparison.Ordinal)
                || text.Contains("Authorize", StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }
}
