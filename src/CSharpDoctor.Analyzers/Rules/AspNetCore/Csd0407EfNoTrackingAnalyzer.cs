using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpDoctor.Analyzers.Rules.AspNetCore;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Csd0407EfNoTrackingAnalyzer : CSharpDoctorAnalyzerBase
{
    protected override RuleDescriptor Descriptor { get; } = new RuleDescriptor(
        "CSD0407",
        "Read-only query without AsNoTracking",
        "AspNetCore",
        DiagnosticSeverity.Info,
        "GET handler '{0}' queries EF without AsNoTracking",
        "Use AsNoTracking() for read-only queries to reduce memory and change-tracking overhead.",
        new[] { "aspnet", "efcore", "performance" });

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

        if (!HasHttpGetAttribute(method) && !method.Identifier.Text.StartsWith("Get", StringComparison.Ordinal))
        {
            return;
        }

        if (!method.ParameterList.Parameters.Any(static p =>
                p.Type?.ToString().Contains("DbContext", StringComparison.Ordinal) == true))
        {
            return;
        }

        var body = method.Body?.ToString() ?? method.ExpressionBody?.ToString() ?? string.Empty;
        if (body.Length == 0)
        {
            return;
        }

        if (body.Contains("AsNoTracking", StringComparison.Ordinal))
        {
            return;
        }

        Report(context, method.Identifier.GetLocation(), method.Identifier.Text);
    }

    private static bool HasHttpGetAttribute(MethodDeclarationSyntax method) =>
        method.AttributeLists
            .SelectMany(static list => list.Attributes)
            .Any(static attr => attr.Name.ToString().Contains("HttpGet", StringComparison.Ordinal));
}
