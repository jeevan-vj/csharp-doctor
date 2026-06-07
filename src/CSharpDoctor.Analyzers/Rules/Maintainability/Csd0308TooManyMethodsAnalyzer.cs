using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpDoctor.Analyzers.Rules.Maintainability;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Csd0308TooManyMethodsAnalyzer : CSharpDoctorAnalyzerBase
{
    private const int MaxMethods = 15;

    protected override RuleDescriptor Descriptor { get; } = new RuleDescriptor(
        "CSD0308",
        "Type has too many methods",
        "Maintainability",
        DiagnosticSeverity.Info,
        "Type '{0}' declares {1} methods",
        "Types with many methods are harder to maintain; extract responsibilities into separate types.",
        new[] { "design", "maintainability" });

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.ClassDeclaration);
        context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.StructDeclaration);
        context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.RecordDeclaration);
    }

    private void Analyze(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not TypeDeclarationSyntax type)
        {
            return;
        }

        var methods = CodeMetricsHelper.CountMethods(type);
        if (methods > MaxMethods)
        {
            Report(context, type.Identifier.GetLocation(), type.Identifier.Text, methods);
        }
    }
}
