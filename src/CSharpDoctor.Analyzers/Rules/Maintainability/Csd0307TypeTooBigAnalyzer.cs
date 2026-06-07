using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpDoctor.Analyzers.Rules.Maintainability;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Csd0307TypeTooBigAnalyzer : CSharpDoctorAnalyzerBase
{
    private const int MaxLines = 120;

    protected override RuleDescriptor Descriptor { get; } = new RuleDescriptor(
        "CSD0307",
        "Type too big",
        "Maintainability",
        DiagnosticSeverity.Info,
        "Type '{0}' has {1} lines of code",
        "Large types often violate single responsibility; consider splitting into smaller types.",
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

        var lines = CodeMetricsHelper.CountLines(type);
        if (lines > MaxLines)
        {
            Report(context, type.Identifier.GetLocation(), type.Identifier.Text, lines);
        }
    }
}
