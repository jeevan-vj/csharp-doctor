using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpDoctor.Analyzers.Rules.Security;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Csd0002WeakCryptoAnalyzer : CSharpDoctorAnalyzerBase
{
    private static readonly string[] WeakTypes = ["MD5", "SHA1", "SHA256Managed", "MD5CryptoServiceProvider"];

    protected override RuleDescriptor Descriptor { get; } = new RuleDescriptor(
        "CSD0002",
        "Weak cryptographic algorithm",
        "Security",
        DiagnosticSeverity.Warning,
        "Weak cryptographic algorithm '{0}' should not be used",
        "Prefer SHA256, SHA384, SHA512, or modern algorithms from System.Security.Cryptography.",
        new[] { "security" });

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeObjectCreation, SyntaxKind.ObjectCreationExpression);
        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    private void AnalyzeObjectCreation(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not ObjectCreationExpressionSyntax creation)
        {
            return;
        }

        var typeName = creation.Type.ToString();
        if (WeakTypes.Any(w => typeName.Contains(w, StringComparison.Ordinal)))
        {
            Report(context, creation.GetLocation(), typeName);
        }
    }

    private void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not InvocationExpressionSyntax invocation)
        {
            return;
        }

        var name = invocation.Expression.ToString();
        if (name.Contains("MD5.Create", StringComparison.Ordinal)
            || name.Contains("SHA1.Create", StringComparison.Ordinal))
        {
            Report(context, invocation.GetLocation(), name);
        }
    }
}
