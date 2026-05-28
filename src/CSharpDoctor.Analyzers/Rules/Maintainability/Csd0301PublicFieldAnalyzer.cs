using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpDoctor.Analyzers.Rules.Maintainability;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Csd0301PublicFieldAnalyzer : CSharpDoctorAnalyzerBase
{
    protected override RuleDescriptor Descriptor { get; } = new RuleDescriptor(
        "CSD0301",
        "Public instance field",
        "Maintainability",
        DiagnosticSeverity.Info,
        "Public field '{0}' should be a property",
        "Expose data through properties to preserve encapsulation and versioning flexibility.",
        new[] { "design", "maintainability" });

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.FieldDeclaration);
    }

    private void Analyze(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not FieldDeclarationSyntax field)
        {
            return;
        }

        if (!field.Modifiers.Any(SyntaxKind.PublicKeyword))
        {
            return;
        }

        if (field.Modifiers.Any(SyntaxKind.StaticKeyword) || field.Modifiers.Any(SyntaxKind.ConstKeyword))
        {
            return;
        }

        if (field.Parent is not (ClassDeclarationSyntax or StructDeclarationSyntax))
        {
            return;
        }

        foreach (var variable in field.Declaration.Variables)
        {
            Report(context, variable.Identifier.GetLocation(), variable.Identifier.Text);
        }
    }
}
