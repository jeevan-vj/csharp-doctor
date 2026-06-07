using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpDoctor.Analyzers.Rules.Correctness;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Csd0120NativeMethodsClassAnalyzer : CSharpDoctorAnalyzerBase
{
    protected override RuleDescriptor Descriptor { get; } = new RuleDescriptor(
        "CSD0120",
        "P/Invoke outside NativeMethods",
        "Correctness",
        DiagnosticSeverity.Info,
        "P/Invoke '{0}' should be declared in a NativeMethods class",
        "Group platform invoke methods in a private static NativeMethods helper class.",
        new[] { "correctness", "design" });

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

        var containingType = method.Ancestors().OfType<TypeDeclarationSyntax>().FirstOrDefault();
        if (containingType?.Identifier.Text.Contains("NativeMethods", StringComparison.Ordinal) == true)
        {
            return;
        }

        Report(context, method.Identifier.GetLocation(), method.Identifier.Text);
    }
}
