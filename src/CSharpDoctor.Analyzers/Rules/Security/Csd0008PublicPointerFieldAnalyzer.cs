using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpDoctor.Analyzers.Rules.Security;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Csd0008PublicPointerFieldAnalyzer : CSharpDoctorAnalyzerBase
{
    protected override RuleDescriptor Descriptor { get; } = new RuleDescriptor(
        "CSD0008",
        "Public pointer field",
        "Security",
        DiagnosticSeverity.Warning,
        "Pointer field '{0}' should not be publicly visible",
        "Public pointer fields expose unsafe memory to callers; keep them private.",
        new[] { "security" });

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

        if (!field.Declaration.Type.ToString().Contains('*'))
        {
            return;
        }

        foreach (var variable in field.Declaration.Variables)
        {
            Report(context, variable.Identifier.GetLocation(), variable.Identifier.Text);
        }
    }
}
