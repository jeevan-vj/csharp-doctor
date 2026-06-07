using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace CSharpDoctor.Analyzers;

/// <summary>
/// Produces a "code only" view of a syntax node: identical text with string/character literal
/// contents and comments blanked out (replaced by spaces, preserving positions). This lets
/// text-based rules match real code without firing on patterns that appear inside string
/// literals or comments (which is what makes the analyzers flag their own source).
/// </summary>
internal static class CodeTextExtractor
{
    public static string ToCodeText(SyntaxNode node)
    {
        var buffer = node.ToFullString().ToCharArray();
        var offset = node.FullSpan.Start;

        foreach (var token in node.DescendantTokens())
        {
            if (IsLiteralToken(token.Kind()))
            {
                Blank(buffer, token.Span, offset);
            }

            foreach (var trivia in token.LeadingTrivia)
            {
                if (IsComment(trivia.Kind()))
                {
                    Blank(buffer, trivia.Span, offset);
                }
            }

            foreach (var trivia in token.TrailingTrivia)
            {
                if (IsComment(trivia.Kind()))
                {
                    Blank(buffer, trivia.Span, offset);
                }
            }
        }

        return new string(buffer);
    }

    private static bool IsLiteralToken(SyntaxKind kind) => kind switch
    {
        SyntaxKind.StringLiteralToken => true,
        SyntaxKind.CharacterLiteralToken => true,
        SyntaxKind.InterpolatedStringTextToken => true,
        SyntaxKind.SingleLineRawStringLiteralToken => true,
        SyntaxKind.MultiLineRawStringLiteralToken => true,
        SyntaxKind.Utf8StringLiteralToken => true,
        SyntaxKind.Utf8SingleLineRawStringLiteralToken => true,
        SyntaxKind.Utf8MultiLineRawStringLiteralToken => true,
        _ => false,
    };

    private static bool IsComment(SyntaxKind kind) => kind switch
    {
        SyntaxKind.SingleLineCommentTrivia => true,
        SyntaxKind.MultiLineCommentTrivia => true,
        SyntaxKind.SingleLineDocumentationCommentTrivia => true,
        SyntaxKind.MultiLineDocumentationCommentTrivia => true,
        _ => false,
    };

    private static void Blank(char[] buffer, TextSpan span, int offset)
    {
        var start = span.Start - offset;
        var end = span.End - offset;
        for (var i = start; i < end && i < buffer.Length; i++)
        {
            if (buffer[i] is not ('\r' or '\n'))
            {
                buffer[i] = ' ';
            }
        }
    }
}
