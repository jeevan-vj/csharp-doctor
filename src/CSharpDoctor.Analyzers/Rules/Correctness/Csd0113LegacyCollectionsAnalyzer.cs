using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpDoctor.Analyzers.Rules.Correctness;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Csd0113LegacyCollectionsAnalyzer : CSharpDoctorAnalyzerBase
{
    private static readonly string[] LegacyTypes = ["ArrayList", "Hashtable", "Queue", "Stack"];

    protected override RuleDescriptor Descriptor { get; } = new RuleDescriptor(
        "CSD0113",
        "Legacy .NET 1.x collection",
        "Correctness",
        DiagnosticSeverity.Warning,
        "Avoid legacy collection type '{0}'",
        "Prefer generic collections from System.Collections.Generic.",
        new[] { "correctness", "maintainability" });

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeObjectCreation, SyntaxKind.ObjectCreationExpression);
        context.RegisterSyntaxNodeAction(AnalyzeType, SyntaxKind.VariableDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeType, SyntaxKind.PropertyDeclaration);
    }

    private void AnalyzeObjectCreation(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not ObjectCreationExpressionSyntax creation)
        {
            return;
        }

        var typeName = creation.Type.ToString();
        if (LegacyTypes.Any(t => typeName.Contains(t, StringComparison.Ordinal)))
        {
            Report(context, creation.GetLocation(), typeName);
        }
    }

    private void AnalyzeType(SyntaxNodeAnalysisContext context)
    {
        string? typeName = context.Node switch
        {
            VariableDeclarationSyntax v => v.Type.ToString(),
            PropertyDeclarationSyntax p => p.Type.ToString(),
            _ => null,
        };

        if (typeName is not null && LegacyTypes.Any(t => typeName.Contains(t, StringComparison.Ordinal)))
        {
            Report(context, context.Node.GetLocation(), typeName);
        }
    }
}
