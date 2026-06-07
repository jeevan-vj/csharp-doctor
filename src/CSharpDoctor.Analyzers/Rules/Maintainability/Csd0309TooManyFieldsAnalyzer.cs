using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpDoctor.Analyzers.Rules.Maintainability;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Csd0309TooManyFieldsAnalyzer : CSharpDoctorAnalyzerBase
{
    private const int MaxFields = 10;

    protected override RuleDescriptor Descriptor { get; } = new RuleDescriptor(
        "CSD0309",
        "Type has too many fields",
        "Maintainability",
        DiagnosticSeverity.Info,
        "Type '{0}' declares {1} instance fields",
        "Many fields suggest the type is doing too much; group related state or extract types.",
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

        var fields = CodeMetricsHelper.CountInstanceFields(type);
        if (fields > MaxFields)
        {
            Report(context, type.Identifier.GetLocation(), type.Identifier.Text, fields);
        }
    }
}
