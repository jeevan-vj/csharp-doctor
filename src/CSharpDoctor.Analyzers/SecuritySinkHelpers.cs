using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpDoctor.Analyzers;

/// <summary>Heuristics for request-derived values and dangerous sinks in security rules.</summary>
public static class SecuritySinkHelpers
{
    private static readonly string[] RequestMemberSuffixes =
    [
        "Query", "Form", "Headers", "RouteValues", "Body", "Cookies", "Path", "Host",
    ];

    private static readonly string[] ShellExecutables =
    [
        "cmd.exe", "powershell", "pwsh", "bash", "sh",
    ];

    public static bool IsLiteralExpression(ExpressionSyntax? expression) => expression switch
    {
        null => false,
        LiteralExpressionSyntax => true,
        ParenthesizedExpressionSyntax p => IsLiteralExpression(p.Expression),
        _ => false,
    };

    public static bool IsDynamicExpression(ExpressionSyntax expression)
    {
        if (IsLiteralExpression(expression))
        {
            return false;
        }

        if (ContainsRequestAccess(expression))
        {
            return true;
        }

        return expression switch
        {
            IdentifierNameSyntax => true,
            MemberAccessExpressionSyntax => true,
            InvocationExpressionSyntax => true,
            InterpolatedStringExpressionSyntax interp => interp.Contents.Any(c => c is not InterpolatedStringTextSyntax),
            BinaryExpressionSyntax add when add.IsKind(SyntaxKind.AddExpression) =>
                IsDynamicExpression(add.Left) || IsDynamicExpression(add.Right),
            _ => !IsLiteralExpression(expression),
        };
    }

    public static bool ContainsRequestAccess(SyntaxNode node) =>
        node.DescendantNodesAndSelf().OfType<MemberAccessExpressionSyntax>()
            .Any(IsRequestMemberAccess);

    public static bool IsRequestMemberAccess(MemberAccessExpressionSyntax memberAccess)
    {
        var name = memberAccess.Name.Identifier.ValueText;
        if (RequestMemberSuffixes.Contains(name, StringComparer.Ordinal))
        {
            return memberAccess.Expression.ToString().Contains("Request", StringComparison.Ordinal)
                || memberAccess.Expression.ToString().Contains("HttpContext", StringComparison.Ordinal);
        }

        return name is "QueryString" or "Route" or "PathBase";
    }

    public static bool IsInsideMethod(SyntaxNode node, out MethodDeclarationSyntax? method)
    {
        method = node.Ancestors().OfType<MethodDeclarationSyntax>().FirstOrDefault();
        return method is not null;
    }

    public static bool MethodContainsText(MethodDeclarationSyntax method, params string[] needles) =>
        needles.Any(n => method.ToString().Contains(n, StringComparison.Ordinal));

    public static bool IsShellExecutable(string? value) =>
        value is not null
        && ShellExecutables.Any(shell => value.Contains(shell, StringComparison.OrdinalIgnoreCase));

    public static bool IsFileSystemSink(MemberAccessExpressionSyntax memberAccess)
    {
        var receiver = memberAccess.Expression.ToString();
        var name = memberAccess.Name.Identifier.ValueText;

        if (receiver is "File" or "Directory" or "System.IO.File" or "System.IO.Directory")
        {
            return name is "ReadAllText" or "ReadAllBytes" or "Open" or "OpenRead" or "OpenWrite"
                or "Create" or "Delete" or "Move" or "Copy" or "Exists" or "GetFiles" or "EnumerateFiles";
        }

        return name is "PhysicalFile" or "SendFileAsync";
    }

    public static bool IsHttpClientSink(MemberAccessExpressionSyntax memberAccess)
    {
        var name = memberAccess.Name.Identifier.ValueText;
        if (name is not ("GetAsync" or "PostAsync" or "PutAsync" or "DeleteAsync" or "SendAsync"
            or "GetStringAsync" or "PostAsJsonAsync"))
        {
            return false;
        }

        var receiver = memberAccess.Expression.ToString();
        return receiver.Contains("HttpClient", StringComparison.OrdinalIgnoreCase)
            || receiver is "client" or "_client" or "httpClient";
    }

    public static bool IsRedirectSink(MemberAccessExpressionSyntax memberAccess) =>
        memberAccess.Name.Identifier.ValueText is "Redirect" or "RedirectPermanent"
            or "RedirectPreserveMethod";

    public static bool IsResultsRedirect(InvocationExpressionSyntax invocation) =>
        invocation.Expression is MemberAccessExpressionSyntax { Name.Identifier.Text: "Redirect" }
        && invocation.Expression.ToString().Contains("Results", StringComparison.Ordinal);
}
