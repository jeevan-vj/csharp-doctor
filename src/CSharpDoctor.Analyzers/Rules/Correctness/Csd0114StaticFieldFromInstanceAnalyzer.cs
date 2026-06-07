using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpDoctor.Analyzers.Rules.Correctness;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Csd0114StaticFieldFromInstanceAnalyzer : CSharpDoctorAnalyzerBase
{
    protected override RuleDescriptor Descriptor { get; } = new RuleDescriptor(
        "CSD0114",
        "Static field assigned from instance method",
        "Correctness",
        DiagnosticSeverity.Warning,
        "Instance method '{0}' assigns static field '{1}'",
        "Avoid mutating static state from instance methods; use static methods or instance fields.",
        new[] { "correctness", "design" });

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.SimpleAssignmentExpression);
    }

    private void Analyze(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not AssignmentExpressionSyntax assignment)
        {
            return;
        }

        var method = assignment.Ancestors().OfType<MethodDeclarationSyntax>().FirstOrDefault();
        if (method is null || method.Modifiers.Any(SyntaxKind.StaticKeyword))
        {
            return;
        }

        var left = assignment.Left.ToString();
        var type = method.Ancestors().OfType<TypeDeclarationSyntax>().FirstOrDefault();
        if (type is null)
        {
            return;
        }

        var staticFields = type.Members
            .OfType<FieldDeclarationSyntax>()
            .Where(f => f.Modifiers.Any(SyntaxKind.StaticKeyword))
            .SelectMany(f => f.Declaration.Variables.Select(v => v.Identifier.Text))
            .ToArray();

        foreach (var field in staticFields)
        {
            if (left == field || left.EndsWith("." + field, StringComparison.Ordinal))
            {
                Report(context, assignment.GetLocation(), method.Identifier.Text, field);
            }
        }
    }
}
