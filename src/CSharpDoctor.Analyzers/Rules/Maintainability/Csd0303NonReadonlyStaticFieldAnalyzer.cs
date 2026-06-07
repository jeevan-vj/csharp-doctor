using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpDoctor.Analyzers.Rules.Maintainability;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Csd0303NonReadonlyStaticFieldAnalyzer : CSharpDoctorAnalyzerBase
{
    protected override RuleDescriptor Descriptor { get; } = new RuleDescriptor(
        "CSD0303",
        "Mutable static field",
        "Maintainability",
        DiagnosticSeverity.Warning,
        "Static field '{0}' should be readonly",
        "Mutable static state is a common source of threading bugs; prefer readonly or thread-safe patterns.",
        new[] { "design", "maintainability", "correctness" });

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

        if (!field.Modifiers.Any(SyntaxKind.StaticKeyword)
            || field.Modifiers.Any(SyntaxKind.ReadOnlyKeyword)
            || field.Modifiers.Any(SyntaxKind.ConstKeyword))
        {
            return;
        }

        foreach (var variable in field.Declaration.Variables)
        {
            Report(context, variable.Identifier.GetLocation(), variable.Identifier.Text);
        }
    }
}
