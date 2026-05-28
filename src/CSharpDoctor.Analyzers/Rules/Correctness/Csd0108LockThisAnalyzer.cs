using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpDoctor.Analyzers.Rules.Correctness;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Csd0108LockThisAnalyzer : CSharpDoctorAnalyzerBase
{
    protected override RuleDescriptor Descriptor { get; } = new RuleDescriptor(
        "CSD0108",
        "lock on this or Type",
        "Correctness",
        DiagnosticSeverity.Warning,
        "Do not lock on '{0}'",
        "Use a private readonly object instance for locking instead of this or typeof.",
        new[] { "correctness" });

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.LockStatement);
    }

    private void Analyze(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not LockStatementSyntax lockStatement)
        {
            return;
        }

        var expression = lockStatement.Expression.ToString();
        if (expression == "this"
            || expression.StartsWith("typeof(", StringComparison.Ordinal))
        {
            Report(context, lockStatement.Expression.GetLocation(), expression);
        }
    }
}
