using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpDoctor.Analyzers.Rules.AspNetCore;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Csd0404FormFileSizeLimitAnalyzer : CSharpDoctorAnalyzerBase
{
    protected override RuleDescriptor Descriptor { get; } = new RuleDescriptor(
        "CSD0404",
        "Upload endpoint without size limit",
        "AspNetCore",
        DiagnosticSeverity.Info,
        "IFormFile parameter '{0}' has no request size limit",
        "Apply RequestSizeLimit, RequestFormLimits, or Kestrel MaxRequestBodySize for upload endpoints.",
        new[] { "aspnet", "security" });

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.Parameter);
    }

    private void Analyze(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not ParameterSyntax parameter)
        {
            return;
        }

        if (parameter.Type?.ToString().Contains("IFormFile", StringComparison.Ordinal) != true)
        {
            return;
        }

        var compilationText = parameter.SyntaxTree.GetRoot().ToString();
        if (compilationText.Contains("RequestSizeLimit", StringComparison.Ordinal)
            || compilationText.Contains("RequestFormLimits", StringComparison.Ordinal)
            || compilationText.Contains("MaxRequestBodySize", StringComparison.Ordinal)
            || compilationText.Contains("DisableRequestSizeLimit", StringComparison.Ordinal))
        {
            return;
        }

        Report(context, parameter.Identifier.GetLocation(), parameter.Identifier.Text);
    }
}
