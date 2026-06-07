using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpDoctor.Analyzers.Rules.Correctness;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Csd0119UnsynchronizedMutableReadAnalyzer : CSharpDoctorAnalyzerBase
{
    protected override RuleDescriptor Descriptor { get; } = new RuleDescriptor(
        "CSD0119",
        "Unsynchronized read of mutable state",
        "Correctness",
        DiagnosticSeverity.Warning,
        "Method '{0}' reads mutable instance state without synchronization",
        "Guard mutable instance fields with locks or use immutable/thread-safe patterns.",
        new[] { "correctness" });

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.MethodDeclaration);
    }

    private void Analyze(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not MethodDeclarationSyntax method)
        {
            return;
        }

        if (method.Modifiers.Any(SyntaxKind.StaticKeyword)
            || method.Modifiers.Any(SyntaxKind.AsyncKeyword))
        {
            return;
        }

        if (method.Body is null && method.ExpressionBody is null)
        {
            return;
        }

        var type = method.Ancestors().OfType<TypeDeclarationSyntax>().FirstOrDefault();
        if (type is null)
        {
            return;
        }

        var mutableFields = type.Members
            .OfType<FieldDeclarationSyntax>()
            .Where(f => !f.Modifiers.Any(SyntaxKind.StaticKeyword)
                && !f.Modifiers.Any(SyntaxKind.ReadOnlyKeyword)
                && !f.Modifiers.Any(SyntaxKind.ConstKeyword))
            .SelectMany(f => f.Declaration.Variables.Select(v => v.Identifier.Text))
            .ToArray();

        if (mutableFields.Length == 0)
        {
            return;
        }

        var body = method.Body ?? (SyntaxNode)method.ExpressionBody!;
        var text = body.ToString();
        if (text.Contains("lock", StringComparison.Ordinal)
            || text.Contains("Monitor.", StringComparison.Ordinal)
            || text.Contains("Semaphore", StringComparison.Ordinal))
        {
            return;
        }

        foreach (var field in mutableFields)
        {
            if (text.Contains(field, StringComparison.Ordinal))
            {
                Report(context, method.Identifier.GetLocation(), method.Identifier.Text);
                return;
            }
        }
    }
}
