using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpDoctor.Analyzers.Rules.Maintainability;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Csd0305MethodTooComplexAnalyzer : CSharpDoctorAnalyzerBase
{
    private const int MaxLines = 40;
    private const int MaxComplexity = 12;

    protected override RuleDescriptor Descriptor { get; } = new RuleDescriptor(
        "CSD0305",
        "Method too big or complex",
        "Maintainability",
        DiagnosticSeverity.Info,
        "Method '{0}' has {1} lines and cyclomatic complexity {2}",
        "Split large or complex methods to improve testability and readability.",
        new[] { "design", "maintainability" });

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

        if (method.Body is null && method.ExpressionBody is null)
        {
            return;
        }

        var lines = CodeMetricsHelper.CountLines(method);
        var complexity = CodeMetricsHelper.CountCyclomaticComplexity(method);

        if (lines > MaxLines || complexity > MaxComplexity)
        {
            Report(context, method.Identifier.GetLocation(), method.Identifier.Text, lines, complexity);
        }
    }
}
