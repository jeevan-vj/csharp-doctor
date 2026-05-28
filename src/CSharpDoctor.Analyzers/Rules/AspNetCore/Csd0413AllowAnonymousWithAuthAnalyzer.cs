using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpDoctor.Analyzers.Rules.AspNetCore;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Csd0413AllowAnonymousWithAuthAnalyzer : CSharpDoctorAnalyzerBase
{
    protected override RuleDescriptor Descriptor { get; } = new RuleDescriptor(
        "CSD0413",
        "AllowAnonymous with authentication configured",
        "AspNetCore",
        DiagnosticSeverity.Info,
        "AllowAnonymous on '{0}' while authentication is registered",
        "Review whether anonymous access is intentional when AddAuthentication is configured.",
        new[] { "aspnet", "security" });

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.ClassDeclaration);
    }

    private void Analyze(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not ClassDeclarationSyntax classDecl)
        {
            return;
        }

        if (!HasAllowAnonymous(classDecl))
        {
            return;
        }

        var compilationText = classDecl.SyntaxTree.GetRoot().ToString();
        if (!compilationText.Contains("AddAuthentication", StringComparison.Ordinal))
        {
            return;
        }

        Report(context, classDecl.GetLocation(), classDecl.Identifier.Text);
    }

    private static bool HasAllowAnonymous(ClassDeclarationSyntax classDecl) =>
        classDecl.AttributeLists
            .SelectMany(static list => list.Attributes)
            .Any(attr => attr.Name.ToString().Contains("AllowAnonymous", StringComparison.Ordinal));
}
