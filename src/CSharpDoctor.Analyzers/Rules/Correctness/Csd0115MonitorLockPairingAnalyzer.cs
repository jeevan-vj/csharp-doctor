using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpDoctor.Analyzers.Rules.Correctness;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Csd0115MonitorLockPairingAnalyzer : CSharpDoctorAnalyzerBase
{
    protected override RuleDescriptor Descriptor { get; } = new RuleDescriptor(
        "CSD0115",
        "Monitor Enter/Exit not paired",
        "Correctness",
        DiagnosticSeverity.Warning,
        "Monitor.{0} should be paired with Monitor.Exit in the same method",
        "Unpaired Monitor.Enter/TryEnter calls can leak locks and cause deadlocks.",
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

        var hasEnter = invocations.Any(i =>
            i is "Monitor.Enter" or "Monitor.TryEnter" or "System.Threading.Monitor.Enter"
                or "System.Threading.Monitor.TryEnter");
        var hasExit = invocations.Any(i =>
            i is "Monitor.Exit" or "System.Threading.Monitor.Exit");

        if (hasEnter && !hasExit)
        {
            var enter = body.DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .First(i => i.Expression.ToString().Contains("Monitor.Enter", StringComparison.Ordinal)
                    || i.Expression.ToString().Contains("Monitor.TryEnter", StringComparison.Ordinal));
            var kind = enter.Expression.ToString().Contains("TryEnter", StringComparison.Ordinal) ? "TryEnter" : "Enter";
            Report(context, enter.GetLocation(), kind);
        }
    }
}
