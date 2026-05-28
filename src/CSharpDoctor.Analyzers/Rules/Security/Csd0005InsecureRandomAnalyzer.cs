using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpDoctor.Analyzers.Rules.Security;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Csd0005InsecureRandomAnalyzer : CSharpDoctorAnalyzerBase
{
    protected override RuleDescriptor Descriptor { get; } = new RuleDescriptor(
        "CSD0005",
        "Insecure random for security-sensitive code",
        "Security",
        DiagnosticSeverity.Warning,
        "System.Random is not cryptographically secure",
        "Use RandomNumberGenerator or RandomNumberGenerator.GetBytes for tokens, keys, and secrets.",
        new[] { "security" });

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeObjectCreation, SyntaxKind.ObjectCreationExpression);
        context.RegisterSyntaxNodeAction(AnalyzeMemberAccess, SyntaxKind.SimpleMemberAccessExpression);
    }

    private void AnalyzeObjectCreation(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not ObjectCreationExpressionSyntax creation)
        {
            return;
        }

        if (!creation.Type.ToString().Contains("Random", StringComparison.Ordinal)
            || creation.Type.ToString().Contains("RandomNumberGenerator", StringComparison.Ordinal))
        {
            return;
        }

        if (IsInSecurityContext(creation))
        {
            Report(context, creation.GetLocation());
        }
    }

    private void AnalyzeMemberAccess(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not MemberAccessExpressionSyntax member)
        {
            return;
        }

        if (member.Expression.ToString() is "Random.Shared" or "Random")
        {
            if (IsInSecurityContext(member))
            {
                Report(context, member.GetLocation());
            }
        }
    }

    private static bool IsInSecurityContext(SyntaxNode node)
    {
        for (var current = node.Parent; current is not null; current = current.Parent)
        {
            var text = current.ToString();
            if (text.Contains("token", StringComparison.OrdinalIgnoreCase)
                || text.Contains("secret", StringComparison.OrdinalIgnoreCase)
                || text.Contains("password", StringComparison.OrdinalIgnoreCase)
                || text.Contains("key", StringComparison.OrdinalIgnoreCase)
                || text.Contains("salt", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
