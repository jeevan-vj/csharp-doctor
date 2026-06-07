using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpDoctor.Analyzers.Rules.Security;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Csd0016PathTraversalAnalyzer : CSharpDoctorAnalyzerBase
{
    protected override RuleDescriptor Descriptor { get; } = new RuleDescriptor(
        "CSD0016",
        "Path traversal via dynamic file path",
        "Security",
        DiagnosticSeverity.Warning,
        "File system access uses a dynamic path: {0}",
        "Validate and canonicalize user-supplied paths before file or directory access.",
        new[] { "security", "aspnet" });

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    private void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not InvocationExpressionSyntax invocation
            || invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
        {
            return;
        }

        if (!SecuritySinkHelpers.IsFileSystemSink(memberAccess))
        {
            return;
        }

        var pathArgument = invocation.ArgumentList.Arguments.FirstOrDefault()?.Expression;
        if (pathArgument is null || !SecuritySinkHelpers.IsDynamicExpression(pathArgument))
        {
            return;
        }

        if (SecuritySinkHelpers.IsInsideMethod(invocation, out var method)
            && SecuritySinkHelpers.MethodContainsText(method!, "GetFullPath", "StartsWith"))
        {
            return;
        }

        Report(context, pathArgument.GetLocation(), memberAccess.Name.Identifier.Text);
    }
}
