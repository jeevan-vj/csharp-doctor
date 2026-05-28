using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpDoctor.Analyzers.Rules.AspNetCore;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Csd0402DbContextSingletonAnalyzer : CSharpDoctorAnalyzerBase
{
    protected override RuleDescriptor Descriptor { get; } = new RuleDescriptor(
        "CSD0402",
        "DbContext registered as singleton",
        "AspNetCore",
        DiagnosticSeverity.Error,
        "DbContext '{0}' must not be registered as Singleton",
        "Register DbContext with AddDbContext (scoped). Singleton DbContext causes threading and stale state issues.",
        new[] { "aspnet", "efcore", "correctness" });

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

        var memberName = GetMemberName(invocation.Expression);
        if (!memberName.Contains("AddSingleton"))
        {
            return;
        }

        var text = invocation.ToString();
        if (!text.Contains("DbContext", StringComparison.Ordinal))
        {
            return;
        }

        var typeName = "DbContext";
        var start = text.IndexOf('<');
        var end = text.IndexOf('>');
        if (start >= 0 && end > start)
        {
            typeName = text.Substring(start + 1, end - start - 1);
        }

        Report(context, invocation.GetLocation(), typeName);
    }

    private static string GetMemberName(SyntaxNode expression) =>
        expression switch
        {
            MemberAccessExpressionSyntax member when member.Name is GenericNameSyntax generic
                => generic.Identifier.Text,
            MemberAccessExpressionSyntax member => member.Name.Identifier.Text,
            _ => expression.ToString(),
        };
}
