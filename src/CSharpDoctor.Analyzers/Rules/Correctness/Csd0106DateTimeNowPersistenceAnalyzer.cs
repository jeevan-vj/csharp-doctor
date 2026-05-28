using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpDoctor.Analyzers.Rules.Correctness;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Csd0106DateTimeNowPersistenceAnalyzer : CSharpDoctorAnalyzerBase
{
    private static readonly string[] PersistenceNames =
    {
        "CreatedAt", "UpdatedAt", "ModifiedAt", "DeletedAt", "Timestamp", "Utc",
    };

    protected override RuleDescriptor Descriptor { get; } = new RuleDescriptor(
        "CSD0106",
        "DateTime.Now for persistence timestamp",
        "Correctness",
        DiagnosticSeverity.Warning,
        "Use DateTime.UtcNow instead of DateTime.Now for '{0}'",
        "Store UTC timestamps in databases and APIs to avoid timezone bugs.",
        new[] { "correctness" });

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.SimpleMemberAccessExpression);
    }

    private void Analyze(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not MemberAccessExpressionSyntax member)
        {
            return;
        }

        if (member.Name.Identifier.Text != "Now"
            || !member.Expression.ToString().Contains("DateTime"))
        {
            return;
        }

        if (!IsPersistenceAssignment(member))
        {
            return;
        }

        Report(context, member.GetLocation(), GetAssignmentTarget(member));
    }

    private static bool IsPersistenceAssignment(MemberAccessExpressionSyntax member)
    {
        if (member.Parent is not AssignmentExpressionSyntax assignment)
        {
            return false;
        }

        var target = assignment.Left.ToString();
        return PersistenceNames.Any(name => target.Contains(name, StringComparison.OrdinalIgnoreCase));
    }

    private static string GetAssignmentTarget(MemberAccessExpressionSyntax member) =>
        member.Parent is AssignmentExpressionSyntax assignment
            ? assignment.Left.ToString()
            : "timestamp";
}
