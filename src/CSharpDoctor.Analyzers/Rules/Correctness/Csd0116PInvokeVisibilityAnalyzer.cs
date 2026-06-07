using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpDoctor.Analyzers.Rules.Correctness;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Csd0116PInvokeVisibilityAnalyzer : CSharpDoctorAnalyzerBase
{
    protected override RuleDescriptor Descriptor { get; } = new RuleDescriptor(
        "CSD0116",
        "P/Invoke should be private static",
        "Correctness",
        DiagnosticSeverity.Warning,
        "P/Invoke method '{0}' should be private and static",
        "Keep platform invoke entry points in a private static NativeMethods helper class.",
        new[] { "correctness", "security" });

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

        if (!method.AttributeLists.SelectMany(a => a.Attributes)
            .Any(a => a.Name.ToString().Contains("DllImport", StringComparison.Ordinal)))
        {
            return;
        }

        var isPublic = method.Modifiers.Any(SyntaxKind.PublicKeyword)
            || method.Modifiers.Any(SyntaxKind.ProtectedKeyword)
            || method.Modifiers.Any(SyntaxKind.InternalKeyword);
        var isStatic = method.Modifiers.Any(SyntaxKind.StaticKeyword);

        if (isPublic || !isStatic)
        {
            Report(context, method.Identifier.GetLocation(), method.Identifier.Text);
        }
    }
}
