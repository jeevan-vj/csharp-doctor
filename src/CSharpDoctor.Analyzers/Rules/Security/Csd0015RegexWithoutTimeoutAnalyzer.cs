using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpDoctor.Analyzers.Rules.Security;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Csd0015RegexWithoutTimeoutAnalyzer : CSharpDoctorAnalyzerBase
{
    protected override RuleDescriptor Descriptor { get; } = new RuleDescriptor(
        "CSD0015",
        "Regex without match timeout",
        "Security",
        DiagnosticSeverity.Warning,
        "Regex constructed without a match timeout",
        "Pass a TimeSpan match timeout to Regex constructors to mitigate ReDoS.",
        new[] { "security" });

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.ObjectCreationExpression);
    }

    private void Analyze(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not ObjectCreationExpressionSyntax creation)
        {
            return;
        }

        var typeName = creation.Type.ToString();
        if (!typeName.Contains("Regex", StringComparison.Ordinal))
        {
            return;
        }

        var text = creation.ToString();
        if (text.Contains("TimeSpan", StringComparison.Ordinal)
            || text.Contains("matchTimeout", StringComparison.Ordinal))
        {
            return;
        }

        Report(context, creation.GetLocation());
    }
}
