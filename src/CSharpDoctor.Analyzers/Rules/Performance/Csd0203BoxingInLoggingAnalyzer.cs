using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpDoctor.Analyzers.Rules.Performance;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Csd0203BoxingInLoggingAnalyzer : CSharpDoctorAnalyzerBase
{
    protected override RuleDescriptor Descriptor { get; } = new RuleDescriptor(
        "CSD0203",
        "Boxing in logging call",
        "Performance",
        DiagnosticSeverity.Info,
        "Value type '{0}' may be boxed in logging call",
        "Use strongly-typed logging overloads or pass primitives without boxing to object parameters.",
        new[] { "performance" });

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

        var name = invocation.Expression.ToString();
        if (!name.Contains("Log", StringComparison.Ordinal))
        {
            return;
        }

        foreach (var arg in invocation.ArgumentList.Arguments)
        {
            var type = context.SemanticModel.GetTypeInfo(arg.Expression).Type;
            if (type is { IsValueType: true, SpecialType: not SpecialType.System_String })
            {
                var conversion = context.SemanticModel.GetConversion(arg.Expression);
                if (conversion.IsBoxing || arg.Expression is LiteralExpressionSyntax)
                {
                    Report(context, arg.GetLocation(), type.Name);
                }
            }
        }
    }
}
