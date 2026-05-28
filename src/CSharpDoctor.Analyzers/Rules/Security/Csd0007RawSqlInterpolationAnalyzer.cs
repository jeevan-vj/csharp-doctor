using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpDoctor.Analyzers.Rules.Security;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Csd0007RawSqlInterpolationAnalyzer : CSharpDoctorAnalyzerBase
{
    private static readonly string[] RawSqlMethods =
    {
        "FromSqlRaw",
        "ExecuteSqlRaw",
        "ExecuteSqlRawAsync",
        "FromSqlInterpolated",
    };

    protected override RuleDescriptor Descriptor { get; } = new RuleDescriptor(
        "CSD0007",
        "Raw SQL with dynamic text",
        "Security",
        DiagnosticSeverity.Warning,
        "Raw SQL method '{0}' uses interpolated or concatenated SQL",
        "Use parameterized SQL (FromSql with FormattableString or parameters) instead of building raw SQL dynamically.",
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

        var methodName = GetMemberName(invocation.Expression);
        if (!RawSqlMethods.Contains(methodName))
        {
            return;
        }

        var sqlArg = invocation.ArgumentList.Arguments.FirstOrDefault()?.Expression;
        if (sqlArg is null)
        {
            return;
        }

        if (!ContainsDynamicSql(sqlArg))
        {
            return;
        }

        Report(context, invocation.GetLocation(), methodName);
    }

    private static string GetMemberName(SyntaxNode expression) =>
        expression switch
        {
            MemberAccessExpressionSyntax member => member.Name.Identifier.Text,
            IdentifierNameSyntax identifier => identifier.Identifier.Text,
            _ => expression.ToString(),
        };

    private static bool ContainsDynamicSql(ExpressionSyntax expression)
    {
        foreach (var node in expression.DescendantNodesAndSelf())
        {
            if (node is InterpolatedStringExpressionSyntax
                || (node is BinaryExpressionSyntax binary && binary.IsKind(SyntaxKind.AddExpression)))
            {
                return true;
            }
        }

        return false;
    }
}
