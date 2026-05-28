using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpDoctor.Analyzers.Rules.Correctness;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Csd0104NullableForgivingMisuseAnalyzer : CSharpDoctorAnalyzerBase
{
    protected override RuleDescriptor Descriptor { get; } = new RuleDescriptor(
        "CSD0104",
        "Nullable forgiving operator misuse",
        "Correctness",
        DiagnosticSeverity.Info,
        "Null-forgiving operator applied to nullable-returning expression",
        "Avoid suppressing nullable flow analysis unless the value is proven non-null.",
        ["correctness", "nullable"]);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.SuppressNullableWarningExpression);
    }

    private void Analyze(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not PostfixUnaryExpressionSyntax { Operand: var operand })
        {
            return;
        }

        var model = context.SemanticModel;
        var typeInfo = model.GetTypeInfo(operand);
        var type = typeInfo.Type;
        if (type is null)
        {
            return;
        }

        var isNullable =
            type.NullableAnnotation == NullableAnnotation.Annotated
            || (operand is InvocationExpressionSyntax invocation
                && model.GetSymbolInfo(invocation).Symbol is IMethodSymbol method
                && method.ReturnType.NullableAnnotation == NullableAnnotation.Annotated);

        if (isNullable)
        {
            Report(context, context.Node.GetLocation(), operand.ToString());
        }
    }
}
