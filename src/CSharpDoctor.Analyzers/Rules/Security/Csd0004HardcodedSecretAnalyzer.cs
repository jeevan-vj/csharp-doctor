using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpDoctor.Analyzers.Rules.Security;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Csd0004HardcodedSecretAnalyzer : CSharpDoctorAnalyzerBase
{
    private static readonly Regex[] SecretPatterns =
    {
        new Regex(@"Password\s*=\s*[^;'""\s]+", RegexOptions.IgnoreCase | RegexOptions.Compiled, TimeSpan.FromMilliseconds(200)),
        new Regex(@"api[_-]?key\s*=\s*[^;'""\s]+", RegexOptions.IgnoreCase | RegexOptions.Compiled, TimeSpan.FromMilliseconds(200)),
        new Regex(@"Bearer\s+eyJ[A-Za-z0-9_-]+\.[A-Za-z0-9_-]+\.[A-Za-z0-9_-]+", RegexOptions.IgnoreCase | RegexOptions.Compiled, TimeSpan.FromMilliseconds(200)),
        new Regex(@"Secret\s*=\s*[^;'""\s]+", RegexOptions.IgnoreCase | RegexOptions.Compiled, TimeSpan.FromMilliseconds(200)),
    };

    protected override RuleDescriptor Descriptor { get; } = new RuleDescriptor(
        "CSD0004",
        "Hardcoded secret in source",
        "Security",
        DiagnosticSeverity.Warning,
        "Possible hardcoded secret in string literal",
        "Move secrets to configuration, user secrets, or a vault. Never commit credentials.",
        new[] { "security" });

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.StringLiteralExpression, SyntaxKind.InterpolatedStringText);
    }

    private void Analyze(SyntaxNodeAnalysisContext context)
    {
        var text = context.Node.ToString().Trim('"');
        if (string.IsNullOrWhiteSpace(text) || text.Length < 8)
        {
            return;
        }

        foreach (var pattern in SecretPatterns)
        {
            if (pattern.IsMatch(text))
            {
                Report(context, context.Node.GetLocation());
                return;
            }
        }
    }
}
