using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpDoctor.Analyzers.Rules.Correctness;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Csd0103SyncOverAsyncAnalyzer : CSharpDoctorAnalyzerBase
{
    protected override RuleDescriptor Descriptor { get; } = new RuleDescriptor(
        "CSD0103",
        "Sync-over-async",
        "Correctness",
        DiagnosticSeverity.Warning,
        "Blocking async operation via '{0}' inside async method",
        "Avoid .Result and .Wait() in async methods. Use await instead.",
        ["correctness", "async"]);

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

        if (member.Name.Identifier.Text is not ("Result" or "Wait" or "GetAwaiter"))
        {
            return;
        }

        if (!IsInsideAsyncMethod(member))
        {
            return;
        }

        Report(context, member.GetLocation(), member.Name.Identifier.Text);
    }

    private static bool IsInsideAsyncMethod(SyntaxNode node)
    {
        for (var current = node.Parent; current is not null; current = current.Parent)
        {
            if (current is MethodDeclarationSyntax { Modifiers: var mods }
                && mods.Any(SyntaxKind.AsyncKeyword))
            {
                return true;
            }
        }

        return false;
    }
}
