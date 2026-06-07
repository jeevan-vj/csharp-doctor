using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpDoctor.Ef.Migrations;

/// <summary>Extracts <c>migrationBuilder.*</c> operations from a migration syntax tree.</summary>
public static class MigrationParser
{
    private static readonly HashSet<string> SupportedKinds = new(StringComparer.Ordinal)
    {
        "DropColumn",
        "AddColumn",
        "AlterColumn",
        "CreateIndex",
        "AddForeignKey",
        "Sql",
    };

    public static IReadOnlyList<MigrationOperation> Parse(SyntaxNode root)
    {
        var operations = new List<MigrationOperation>();

        foreach (var invocation in root.DescendantNodes().OfType<InvocationExpressionSyntax>())
        {
            if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
            {
                continue;
            }

            var kind = memberAccess.Name.Identifier.ValueText;
            if (!SupportedKinds.Contains(kind))
            {
                continue;
            }

            operations.Add(kind switch
            {
                "DropColumn" => new MigrationOperation(
                    Kind: kind,
                    Table: ArgumentValue(invocation, "table", 1),
                    Column: ArgumentValue(invocation, "name", 0),
                    Nullable: null,
                    ColumnType: null,
                    OldColumnType: null,
                    DefaultValue: null,
                    DefaultValueSql: null,
                    Sql: null,
                    Location: invocation.GetLocation()),
                "AddColumn" or "AlterColumn" => new MigrationOperation(
                    Kind: kind,
                    Table: ArgumentValue(invocation, "table", 1),
                    Column: ArgumentValue(invocation, "name", 0),
                    Nullable: ArgumentBool(invocation, "nullable"),
                    ColumnType: ArgumentValue(invocation, "type", 2),
                    OldColumnType: kind == "AlterColumn" ? ArgumentValue(invocation, "oldType", 3) : null,
                    DefaultValue: ArgumentValue(invocation, "defaultValue", positionalIndex: null),
                    DefaultValueSql: ArgumentValue(invocation, "defaultValueSql", positionalIndex: null),
                    Sql: null,
                    Location: invocation.GetLocation()),
                "Sql" => new MigrationOperation(
                    Kind: kind,
                    Table: null,
                    Column: null,
                    Nullable: null,
                    ColumnType: null,
                    OldColumnType: null,
                    DefaultValue: null,
                    DefaultValueSql: null,
                    Sql: ArgumentExpression(invocation, 0),
                    Location: invocation.GetLocation()),
                _ => new MigrationOperation(
                    kind,
                    ArgumentValue(invocation, "table", 0),
                    ArgumentValue(invocation, "column", 1),
                    null, null, null, null, null, null,
                    invocation.GetLocation()),
            });
        }

        return operations;
    }

    private static string? ArgumentValue(
        InvocationExpressionSyntax invocation,
        string parameterName,
        int? positionalIndex)
    {
        var arguments = invocation.ArgumentList.Arguments;

        var named = arguments.FirstOrDefault(a => a.NameColon?.Name.Identifier.ValueText == parameterName);
        if (named is not null)
        {
            return LiteralOrTypeName(named.Expression);
        }

        if (positionalIndex is not null
            && arguments.All(static a => a.NameColon is null)
            && positionalIndex.Value < arguments.Count)
        {
            return LiteralOrTypeName(arguments[positionalIndex.Value].Expression);
        }

        return null;
    }

    private static bool? ArgumentBool(InvocationExpressionSyntax invocation, string parameterName)
    {
        var value = ArgumentValue(invocation, parameterName, positionalIndex: null);
        return value switch
        {
            "true" => true,
            "false" => false,
            _ => null,
        };
    }

    private static string? ArgumentExpression(InvocationExpressionSyntax invocation, int index)
    {
        var arguments = invocation.ArgumentList.Arguments;
        return index < arguments.Count ? arguments[index].Expression.ToString() : null;
    }

    private static string? LiteralOrTypeName(ExpressionSyntax expression) => expression switch
    {
        LiteralExpressionSyntax literal => literal.Token.ValueText,
        TypeOfExpressionSyntax typeOf => typeOf.Type.ToString(),
        _ => expression.ToString().Trim('"'),
    };
}
