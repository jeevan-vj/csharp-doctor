namespace CSharpDoctor.Core.Diagnostics;

/// <summary>Builds stable diagnostic identities for baselines and deduplication.</summary>
public static class DiagnosticIdentity
{
    /// <summary>Format: filePath::line:column::plugin/rule</summary>
    public static string Build(DiagnosticRecord diagnostic) =>
        Build(
            diagnostic.FilePath,
            diagnostic.Line,
            diagnostic.Column,
            diagnostic.Plugin,
            diagnostic.Rule);

    public static string Build(
        string filePath,
        int line,
        int column,
        string plugin,
        string rule) =>
        $"{NormalizePath(filePath)}::{line}:{column}::{plugin}/{rule}";

    private static string NormalizePath(string filePath) =>
        filePath.Replace('\\', '/');
}
