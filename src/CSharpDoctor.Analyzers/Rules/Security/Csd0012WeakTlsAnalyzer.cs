using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpDoctor.Analyzers.Rules.Security;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Csd0012WeakTlsAnalyzer : CSharpDoctorAnalyzerBase
{
    protected override RuleDescriptor Descriptor { get; } = new RuleDescriptor(
        "CSD0012",
        "Weak TLS protocol enabled",
        "Security",
        DiagnosticSeverity.Warning,
        "ServicePointManager enables weak TLS: {0}",
        "Use TLS 1.2 or TLS 1.3 only. Remove Tls and Tls11 from SecurityProtocol.",
        new[] { "security" });

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.CompilationUnit);
    }

    private void Analyze(SyntaxNodeAnalysisContext context)
    {
        var text = context.Node.ToString();
        if (!text.Contains("SecurityProtocol", StringComparison.Ordinal))
        {
            return;
        }

        if (text.Contains("SecurityProtocolType.Tls11", StringComparison.Ordinal)
            || text.Contains("SecurityProtocolType.Ssl3", StringComparison.Ordinal)
            || HasStandaloneTls(text))
        {
            Report(context, context.Node.GetLocation(), "SecurityProtocol");
        }
    }

    private static bool HasStandaloneTls(string text)
    {
        const string marker = "SecurityProtocolType.Tls";
        var index = 0;
        while ((index = text.IndexOf(marker, index, StringComparison.Ordinal)) >= 0)
        {
            var nextIndex = index + marker.Length;
            if (nextIndex >= text.Length || !char.IsDigit(text[nextIndex]))
            {
                return true;
            }

            index = nextIndex;
        }

        return false;
    }
}
