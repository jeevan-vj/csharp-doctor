using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpDoctor.Analyzers.Rules.Correctness;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Csd0105AsyncVoidAnalyzer : CSharpDoctorAnalyzerBase
{
    protected override RuleDescriptor Descriptor { get; } = new RuleDescriptor(
        "CSD0105",
        "async void method",
        "Correctness",
        DiagnosticSeverity.Warning,
        "async void method '{0}' can crash the process on unhandled exceptions",
        "Use async Task instead of async void except for event handlers.",
        new[] { "correctness", "async" });

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

        if (!method.Modifiers.Any(SyntaxKind.AsyncKeyword))
        {
            return;
        }

        if (method.ReturnType.ToString() != "void")
        {
            return;
        }

        if (IsEventHandler(method))
        {
            return;
        }

        Report(context, method.Identifier.GetLocation(), method.Identifier.Text);
    }

    private static bool IsEventHandler(MethodDeclarationSyntax method)
    {
        if (method.Identifier.Text is not ("OnClick" or "OnChanged" or "OnClosed"))
        {
            return method.ParameterList.Parameters.Count == 2
                && method.ParameterList.Parameters[0].Type?.ToString().Contains("object") == true
                && method.ParameterList.Parameters[1].Type?.ToString().Contains("EventArgs") == true;
        }

        return true;
    }
}
