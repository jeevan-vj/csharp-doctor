using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpDoctor.Analyzers.Rules.Correctness;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Csd0102DisposableNotUsedAnalyzer : CSharpDoctorAnalyzerBase
{
    protected override RuleDescriptor Descriptor { get; } = new RuleDescriptor(
        "CSD0102",
        "IDisposable not in using statement",
        "Correctness",
        DiagnosticSeverity.Warning,
        "Disposable '{0}' should be wrapped in using or await using",
        "Wrap IDisposable instances in using or await using to ensure deterministic disposal.",
        new[] { "correctness" });

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.LocalDeclarationStatement, SyntaxKind.ExpressionStatement);
    }

    private void Analyze(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is LocalDeclarationStatementSyntax local
            && !local.UsingKeyword.IsKind(SyntaxKind.UsingKeyword)
            && local.Parent is not UsingStatementSyntax)
        {
            foreach (var declarator in local.Declaration.Variables)
            {
                if (declarator.Initializer?.Value is ObjectCreationExpressionSyntax creation
                    && IsDisposable(creation, context.SemanticModel))
                {
                    Report(context, creation.GetLocation(), creation.Type.ToString());
                }
            }
        }
        else if (context.Node is ExpressionStatementSyntax { Expression: ObjectCreationExpressionSyntax creation }
            && context.Node.Parent is not UsingStatementSyntax
            && IsDisposable(creation, context.SemanticModel))
        {
            Report(context, creation.GetLocation(), creation.Type.ToString());
        }
    }

    private static bool IsDisposable(ObjectCreationExpressionSyntax creation, SemanticModel model)
    {
        var type = model.GetTypeInfo(creation).Type;
        return type?.AllInterfaces.Any(i => i.Name == "IDisposable") == true
            || type?.Name is "FileStream" or "StreamReader" or "StreamWriter";
    }
}
