using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpDoctor.Analyzers.Rules.Correctness;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Csd0112NotImplementedExceptionAnalyzer : CSharpDoctorAnalyzerBase
{
    protected override RuleDescriptor Descriptor { get; } = new RuleDescriptor(
        "CSD0112",
        "NotImplementedException in concrete method",
        "Correctness",
        DiagnosticSeverity.Warning,
        "Method '{0}' throws NotImplementedException but is not abstract or an interface member",
        "Implement the method or mark the containing type abstract; do not ship NotImplementedException stubs.",
        new[] { "correctness", "design" });

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.ThrowStatement);
    }

    private void Analyze(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not ThrowStatementSyntax throwStatement)
        {
            return;
        }

        var expression = throwStatement.Expression?.ToString() ?? string.Empty;
        if (!expression.Contains("NotImplementedException", StringComparison.Ordinal))
        {
            return;
        }

        var method = throwStatement.Ancestors().OfType<BaseMethodDeclarationSyntax>().FirstOrDefault();
        if (method is null)
        {
            return;
        }

        if (method.Modifiers.Any(SyntaxKind.AbstractKeyword))
        {
            return;
        }

        var containingType = method.Ancestors().OfType<TypeDeclarationSyntax>().FirstOrDefault();
        if (containingType?.Modifiers.Any(SyntaxKind.AbstractKeyword) == true
            && method is MethodDeclarationSyntax { Modifiers: var mods }
            && mods.Any(SyntaxKind.AbstractKeyword))
        {
            return;
        }

        var methodName = method switch
        {
            MethodDeclarationSyntax m => m.Identifier.Text,
            ConstructorDeclarationSyntax => ".ctor",
            _ => "method",
        };

        Report(context, throwStatement.GetLocation(), methodName);
    }
}
