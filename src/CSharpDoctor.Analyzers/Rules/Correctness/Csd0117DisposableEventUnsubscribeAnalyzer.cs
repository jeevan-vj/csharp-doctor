using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpDoctor.Analyzers.Rules.Correctness;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Csd0117DisposableEventUnsubscribeAnalyzer : CSharpDoctorAnalyzerBase
{
    protected override RuleDescriptor Descriptor { get; } = new RuleDescriptor(
        "CSD0117",
        "Disposable type subscribes to event without unsubscribe",
        "Correctness",
        DiagnosticSeverity.Warning,
        "Type '{0}' subscribes to events but does not unsubscribe in Dispose",
        "Unsubscribe from events in Dispose to prevent leaks from long-lived publishers.",
        new[] { "correctness" });

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.ClassDeclaration);
    }

    private void Analyze(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not ClassDeclarationSyntax classDecl)
        {
            return;
        }

        var implementsDisposable = classDecl.BaseList?.Types
            .Any(t => t.Type.ToString().Contains("IDisposable", StringComparison.Ordinal)) == true
            || classDecl.Members.OfType<MethodDeclarationSyntax>()
                .Any(m => m.Identifier.Text == "Dispose");

        if (!implementsDisposable)
        {
            return;
        }

        var hasSubscribe = classDecl.DescendantNodes()
            .OfType<AssignmentExpressionSyntax>()
            .Any(a => a.OperatorToken.IsKind(SyntaxKind.PlusEqualsToken));

        if (!hasSubscribe)
        {
            return;
        }

        var disposeMethod = classDecl.Members
            .OfType<MethodDeclarationSyntax>()
            .FirstOrDefault(m => m.Identifier.Text == "Dispose");

        var hasUnsubscribe = disposeMethod?.DescendantNodes()
            .OfType<AssignmentExpressionSyntax>()
            .Any(a => a.OperatorToken.IsKind(SyntaxKind.MinusEqualsToken)) == true;

        if (!hasUnsubscribe)
        {
            Report(context, classDecl.Identifier.GetLocation(), classDecl.Identifier.Text);
        }
    }
}
