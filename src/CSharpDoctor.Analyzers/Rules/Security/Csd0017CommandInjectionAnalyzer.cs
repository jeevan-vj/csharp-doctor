using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpDoctor.Analyzers.Rules.Security;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Csd0017CommandInjectionAnalyzer : CSharpDoctorAnalyzerBase
{
    protected override RuleDescriptor Descriptor { get; } = new RuleDescriptor(
        "CSD0017",
        "Command injection via Process.Start",
        "Security",
        DiagnosticSeverity.Warning,
        "Process.Start uses dynamic command input: {0}",
        "Avoid shelling out with user-controlled arguments; use argument arrays and fixed executables.",
        new[] { "security" });

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.InvocationExpression);
    }

    private void Analyze(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not InvocationExpressionSyntax invocation)
        {
            return;
        }

        var text = invocation.Expression.ToString();
        if (!text.Contains("Process.Start", StringComparison.Ordinal))
        {
            return;
        }

        foreach (var argument in invocation.ArgumentList.Arguments)
        {
            var name = argument.NameColon?.Name.Identifier.ValueText;
            var expr = argument.Expression;

            if (name is "fileName" or "FileName" && SecuritySinkHelpers.IsDynamicExpression(expr))
            {
                Report(context, expr.GetLocation(), "FileName");
                return;
            }

            if (name is "arguments" or "Arguments" && SecuritySinkHelpers.IsDynamicExpression(expr))
            {
                Report(context, expr.GetLocation(), "Arguments");
                return;
            }

            if (argument.NameColon is null && SecuritySinkHelpers.IsDynamicExpression(expr))
            {
                Report(context, expr.GetLocation(), expr.ToString());
                return;
            }

            if (SecuritySinkHelpers.IsShellExecutable(expr.ToString().Trim('"')))
            {
                Report(context, expr.GetLocation(), "shell executable");
                return;
            }
        }
    }
}
