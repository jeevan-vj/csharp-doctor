using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpDoctor.Analyzers.Rules.Maintainability;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Csd0310UnmaintainableCodeAnalyzer : CSharpDoctorAnalyzerBase
{
    private const int MaxLines = 60;
    private const int MaxComplexity = 20;

    protected override RuleDescriptor Descriptor { get; } = new RuleDescriptor(
        "CSD0310",
        "Unmaintainable method",
        "Maintainability",
        DiagnosticSeverity.Warning,
        "Method '{0}' is difficult to maintain (lines: {1}, complexity: {2})",
        "Refactor very large or very complex methods before they become change hotspots.",
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
