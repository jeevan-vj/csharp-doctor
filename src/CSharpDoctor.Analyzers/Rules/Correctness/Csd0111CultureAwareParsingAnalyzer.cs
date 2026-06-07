using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpDoctor.Analyzers.Rules.Correctness;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Csd0111CultureAwareParsingAnalyzer : CSharpDoctorAnalyzerBase
{
    private static readonly string[] ParseMethods =
    [
        "Parse", "TryParse", "ToString",
    ];

    private static readonly string[] ParseTypes =
    [
        "int", "long", "float", "double", "decimal", "DateTime", "DateOnly", "TimeOnly",
        "Int32", "Int64", "Single", "Double", "Decimal",
    ];

    protected override RuleDescriptor Descriptor { get; } = new RuleDescriptor(
        "CSD0111",
        "Parsing without culture",
        "Correctness",
        DiagnosticSeverity.Warning,
        "Parsing or formatting '{0}' should specify a culture (prefer CultureInfo.InvariantCulture)",
        "Use culture-aware overloads to avoid locale-dependent bugs in servers and persisted data.",
        new[] { "correctness" });

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

        var expression = invocation.Expression.ToString();
        if (!ParseTypes.Any(t => expression.StartsWith(t + ".", StringComparison.Ordinal))
            && !expression.StartsWith("Convert.To", StringComparison.Ordinal))
        {
            return;
        }

        if (!ParseMethods.Any(m => expression.EndsWith("." + m, StringComparison.Ordinal)))
        {
            return;
        }

        if (expression.Contains("InvariantCulture", StringComparison.Ordinal)
            || expression.Contains("CurrentCulture", StringComparison.Ordinal)
            || expression.Contains("CultureInfo", StringComparison.Ordinal))
        {
            return;
        }

        if (HasCultureArgument(invocation))
        {
            return;
        }

        Report(context, invocation.GetLocation(), expression);
    }

    private static bool HasCultureArgument(InvocationExpressionSyntax invocation)
    {
        foreach (var arg in invocation.ArgumentList.Arguments)
        {
            var text = arg.Expression.ToString();
            if (text.Contains("Culture", StringComparison.Ordinal)
                || text.Contains("IFormatProvider", StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }
}
