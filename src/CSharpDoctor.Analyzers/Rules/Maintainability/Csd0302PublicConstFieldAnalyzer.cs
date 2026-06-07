using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpDoctor.Analyzers.Rules.Maintainability;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Csd0302PublicConstFieldAnalyzer : CSharpDoctorAnalyzerBase
{
    protected override RuleDescriptor Descriptor { get; } = new RuleDescriptor(
        "CSD0302",
        "Public constant field",
        "Maintainability",
        DiagnosticSeverity.Info,
        "Avoid public constant field '{0}'",
        "Public constants are part of your assembly contract; prefer static readonly properties for versioning flexibility.",
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

        if (!field.Modifiers.Any(SyntaxKind.PublicKeyword)
            || !field.Modifiers.Any(SyntaxKind.ConstKeyword))
        {
            return;
        }

        foreach (var variable in field.Declaration.Variables)
        {
            Report(context, variable.Identifier.GetLocation(), variable.Identifier.Text);
        }
    }
}
