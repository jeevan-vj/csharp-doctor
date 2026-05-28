using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpDoctor.Analyzers.Rules.Performance;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Csd0205MultipleEnumerationAnalyzer : CSharpDoctorAnalyzerBase
{
    private static readonly string[] EnumeratingMethods = { "Count", "Any", "First", "Last", "Single" };

    protected override RuleDescriptor Descriptor { get; } = new RuleDescriptor(
        "CSD0205",
        "Multiple enumeration of IEnumerable",
        "Performance",
        DiagnosticSeverity.Warning,
        "IEnumerable '{0}' may be enumerated multiple times",
        "Materialize with ToList/ToArray once, or use a single pass.",
        new[] { "performance" });

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeMethod, SyntaxKind.MethodDeclaration);
    }

    private void AnalyzeMethod(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not MethodDeclarationSyntax method)
        {
            return;
        }

        var parameters = method.ParameterList.Parameters
            .Where(p => p.Type?.ToString().Contains("IEnumerable") == true)
            .Select(p => p.Identifier.Text)
            .ToList();

        if (parameters.Count == 0)
        {
            return;
        }

        var body = method.Body ?? (SyntaxNode?)method.ExpressionBody;
        if (body is null)
        {
            return;
        }

        var text = body.ToString();
        foreach (var parameter in parameters)
        {
            var hasLinqCall = EnumeratingMethods.Any(m => text.Contains($"{parameter}.{m}("));
            var hasForeach = text.Contains($"foreach") && text.Contains($"in {parameter}");
            if (hasLinqCall && hasForeach)
            {
                Report(context, method.Identifier.GetLocation(), parameter);
            }
        }
    }
}
