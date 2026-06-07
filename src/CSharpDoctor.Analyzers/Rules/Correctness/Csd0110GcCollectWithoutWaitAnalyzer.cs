using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpDoctor.Analyzers.Rules.Correctness;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Csd0110GcCollectWithoutWaitAnalyzer : CSharpDoctorAnalyzerBase
{
    protected override RuleDescriptor Descriptor { get; } = new RuleDescriptor(
        "CSD0110",
        "GC.Collect without WaitForPendingFinalizers",
        "Correctness",
        DiagnosticSeverity.Warning,
        "GC.Collect() should be paired with GC.WaitForPendingFinalizers() in the same method",
        "If you must call GC.Collect(), also call GC.WaitForPendingFinalizers() in the same method.",
        new[] { "correctness" });

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeMethod, SyntaxKind.MethodDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeMethod, SyntaxKind.ConstructorDeclaration);
    }

    private void AnalyzeMethod(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not BaseMethodDeclarationSyntax method)
        {
            return;
        }

        var body = method.Body ?? (SyntaxNode?)method.ExpressionBody;
        if (body is null)
        {
            return;
        }

        var text = body.ToString();
        if (text.Contains("GC.Collect", StringComparison.Ordinal)
            && !text.Contains("GC.WaitForPendingFinalizers", StringComparison.Ordinal))
        {
            var collectInvocation = body.DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .FirstOrDefault(i => i.Expression.ToString() is "GC.Collect" or "System.GC.Collect");

            if (collectInvocation is not null)
            {
                Report(context, collectInvocation.GetLocation());
            }
        }
    }
}
