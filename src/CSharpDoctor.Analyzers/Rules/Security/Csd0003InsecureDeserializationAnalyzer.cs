using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpDoctor.Analyzers.Rules.Security;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Csd0003InsecureDeserializationAnalyzer : CSharpDoctorAnalyzerBase
{
    protected override RuleDescriptor Descriptor { get; } = new RuleDescriptor(
        "CSD0003",
        "Insecure deserialization API",
        "Security",
        DiagnosticSeverity.Error,
        "Insecure deserialization via '{0}'",
        "Avoid BinaryFormatter and untyped JsonSerializer.Deserialize overloads for untrusted input.",
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
        if (typeName.Contains("BinaryFormatter", StringComparison.Ordinal))
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

        var text = CodeTextExtractor.ToCodeText(invocation);
        if (text.Contains("JsonSerializer.Deserialize<object>", StringComparison.Ordinal)
            || text.Contains("JsonSerializer.Deserialize(typeof(object)", StringComparison.Ordinal))
        {
            Report(context, invocation.GetLocation(), text);
        }
    }
}
