using Microsoft.CodeAnalysis;

namespace CSharpDoctor.Ef.Migrations;

/// <summary>A single operation invoked on a <c>MigrationBuilder</c> inside a migration's <c>Up</c> method.</summary>
public sealed record MigrationOperation(
    string Kind,
    string? Table,
    string? Column,
    bool? Nullable,
    string? ColumnType,
    string? OldColumnType,
    string? DefaultValue,
    string? DefaultValueSql,
    string? Sql,
    Location Location);
