using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpDoctor.Analyzers.Rules.Security;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Csd0014SecretInAttributeAnalyzer : CSharpDoctorAnalyzerBase
{
    private static readonly string[] SecretPatterns =
    {
        "Password=",
        "Pwd=",
        "ApiKey=",
        "Secret=",
        "Token=",
        "Bearer ",
        "AccountKey=",
    };

    protected override RuleDescriptor Descriptor { get; } = new RuleDescriptor(
        "CSD0014",
        "Secret in attribute argument",
        "Security",
        DiagnosticSeverity.Warning,
        "Attribute argument may contain a hardcoded secret",
        "Move secrets to configuration, user secrets, or a vault. Do not embed credentials in source attributes.",
        new[] { "security" });

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.Attribute);
    }

    private void Analyze(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not AttributeSyntax attribute)
        {
            return;
        }

        var text = attribute.ToString();
        foreach (var pattern in SecretPatterns)
        {
            if (text.Contains(pattern, StringComparison.OrdinalIgnoreCase))
            {
                Report(context, attribute.GetLocation());
                return;
            }
        }
    }
}
