using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpDoctor.Analyzers.Rules.Security;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Csd0006EmptyCatchAnalyzer : CSharpDoctorAnalyzerBase
{
    protected override RuleDescriptor Descriptor { get; } = new RuleDescriptor(
        "CSD0006",
        "Empty catch block",
        "Security",
        DiagnosticSeverity.Warning,
        "Empty catch block swallows exceptions",
        "Log, rethrow, or handle the exception. Empty catch blocks hide failures.",
        new[] { "security", "correctness" });

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.CatchClause);
    }

    private void Analyze(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not CatchClauseSyntax catchClause)
        {
            return;
        }

        if (catchClause.Block?.Statements.Count > 0)
        {
            return;
        }

        if (catchClause.Filter is not null)
        {
            return;
        }

        Report(context, catchClause.GetLocation());
    }
}
