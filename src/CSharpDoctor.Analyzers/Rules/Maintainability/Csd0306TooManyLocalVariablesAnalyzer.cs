using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpDoctor.Analyzers.Rules.Maintainability;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Csd0306TooManyLocalVariablesAnalyzer : CSharpDoctorAnalyzerBase
{
    private const int MaxLocals = 8;

    protected override RuleDescriptor Descriptor { get; } = new RuleDescriptor(
        "CSD0306",
        "Too many local variables",
        "Maintainability",
        DiagnosticSeverity.Info,
        "Method '{0}' declares {1} local variables",
        "Extract helper methods or introduce a parameter object when local variable count grows.",
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

        var locals = CodeMetricsHelper.CountLocalVariables(method);
        if (locals > MaxLocals)
        {
            Report(context, method.Identifier.GetLocation(), method.Identifier.Text, locals);
        }
    }
}
