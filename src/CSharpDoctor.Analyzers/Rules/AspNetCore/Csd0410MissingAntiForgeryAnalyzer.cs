using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpDoctor.Analyzers.Rules.AspNetCore;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Csd0410MissingAntiForgeryAnalyzer : CSharpDoctorAnalyzerBase
{
    protected override RuleDescriptor Descriptor { get; } = new RuleDescriptor(
        "CSD0410",
        "HttpPost without antiforgery",
        "AspNetCore",
        DiagnosticSeverity.Warning,
        "POST action '{0}' has no antiforgery validation",
        "Add [ValidateAntiForgeryToken] or equivalent for state-changing MVC actions.",
        new[] { "aspnet", "security" });

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

        if (!HasAttribute(method, "HttpPost"))
        {
            return;
        }

        if (HasAntiForgery(method))
        {
            return;
        }

        Report(context, method.GetLocation(), method.Identifier.Text);
    }

    private static bool HasAttribute(MethodDeclarationSyntax method, string name) =>
        method.AttributeLists
            .SelectMany(static list => list.Attributes)
            .Any(attr => attr.Name.ToString().Contains(name, StringComparison.Ordinal));

    private static bool HasAntiForgery(MethodDeclarationSyntax method)
    {
        if (HasAttribute(method, "ValidateAntiForgeryToken")
            || HasAttribute(method, "AutoValidateAntiforgeryToken")
            || HasAttribute(method, "IgnoreAntiforgeryToken"))
        {
            return true;
        }

        return method.Ancestors()
            .OfType<ClassDeclarationSyntax>()
            .Any(cls => HasAttributeOnType(cls, "ValidateAntiForgeryToken")
                        || HasAttributeOnType(cls, "AutoValidateAntiforgeryToken"));
    }

    private static bool HasAttributeOnType(ClassDeclarationSyntax cls, string name) =>
        cls.AttributeLists
            .SelectMany(static list => list.Attributes)
            .Any(attr => attr.Name.ToString().Contains(name, StringComparison.Ordinal));
}
