using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpDoctor.Analyzers.Rules.Correctness;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Csd0118ReaderWriterLockPairingAnalyzer : CSharpDoctorAnalyzerBase
{
    protected override RuleDescriptor Descriptor { get; } = new RuleDescriptor(
        "CSD0118",
        "ReaderWriterLock not released",
        "Correctness",
        DiagnosticSeverity.Warning,
        "ReaderWriterLock acquire should be paired with Release in the same method",
        "Unreleased ReaderWriterLock locks can deadlock other threads.",
        new[] { "correctness" });

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.MethodDeclaration);
    }

    private void Analyze(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not MethodDeclarationSyntax method)
        {
            return;
        }

        var body = method.Body ?? (SyntaxNode?)method.ExpressionBody;
        if (body is null)
        {
            return;
        }

        var invocations = body.DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .Select(i => i.Expression.ToString())
            .ToList();

        var hasAcquire = invocations.Any(i =>
            i.Contains("AcquireReaderLock", StringComparison.Ordinal)
            || i.Contains("AcquireWriterLock", StringComparison.Ordinal));
        var hasRelease = invocations.Any(i => i.Contains("ReleaseLock", StringComparison.Ordinal));

        if (hasAcquire && !hasRelease)
        {
            var acquire = body.DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .First(i => i.Expression.ToString().Contains("Acquire", StringComparison.Ordinal));
            Report(context, acquire.GetLocation());
        }
    }
}
