using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpDoctor.Analyzers.Rules.AspNetCore;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Csd0409SwaggerExposureAnalyzer : CSharpDoctorAnalyzerBase
{
    protected override RuleDescriptor Descriptor { get; } = new RuleDescriptor(
        "CSD0409",
        "Swagger exposed outside Development",
        "AspNetCore",
        DiagnosticSeverity.Warning,
        "UseSwagger/UseSwaggerUI called without Development guard",
        "Restrict Swagger to Development or protect it with authentication.",
        new[] { "aspnet", "security" });

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.CompilationUnit);
    }

    private void Analyze(SyntaxNodeAnalysisContext context)
    {
        var text = context.Node.ToString();
        if (!text.Contains("UseSwagger", StringComparison.Ordinal)
            && !text.Contains("UseSwaggerUI", StringComparison.Ordinal))
        {
            return;
        }

        if (text.Contains("IsDevelopment", StringComparison.Ordinal))
        {
            return;
        }

        Report(context, context.Node.GetLocation());
    }
}
