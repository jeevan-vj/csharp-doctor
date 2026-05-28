namespace CSharpDoctor.Core.Suppression;

/// <summary>Evaluates csharp-doctor-disable inline comments.</summary>
public static class SuppressionEvaluator
{
    public const string DisableLineMarker = "csharp-doctor-disable-line";
    public const string DisableNextLineMarker = "csharp-doctor-disable-next-line";

    public static SuppressionResult Evaluate(
        IReadOnlyList<string> fileLines,
        int diagnosticLine,
        string ruleId)
    {
        if (fileLines.Count == 0 || diagnosticLine <= 0)
        {
            return new SuppressionResult(false);
        }

        var lineIndex = diagnosticLine - 1;
        if (lineIndex < fileLines.Count && ContainsDisable(fileLines[lineIndex], ruleId))
        {
            return new SuppressionResult(true);
        }

        if (lineIndex > 0 && ContainsDisable(fileLines[lineIndex - 1], ruleId, nextLineOnly: true))
        {
            return new SuppressionResult(true);
        }

        if (lineIndex > 0 && LooksLikeNearMiss(fileLines[lineIndex - 1]))
        {
            return new SuppressionResult(false, NearMissHint: $"Add '{DisableNextLineMarker}' on the line above to suppress {ruleId}.");
        }

        return new SuppressionResult(false);
    }

    private static bool ContainsDisable(string line, string ruleId, bool nextLineOnly = false)
    {
        if (!line.Contains("csharp-doctor-disable", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (nextLineOnly && !line.Contains(DisableNextLineMarker, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (line.Contains("csharp-doctor-disable", StringComparison.OrdinalIgnoreCase)
            && !line.Contains(ruleId, StringComparison.OrdinalIgnoreCase)
            && line.Contains(':'))
        {
            return false;
        }

        return line.Contains(DisableLineMarker, StringComparison.OrdinalIgnoreCase)
            || line.Contains(DisableNextLineMarker, StringComparison.OrdinalIgnoreCase)
            || line.Contains($"csharp-doctor-disable {ruleId}", StringComparison.OrdinalIgnoreCase)
            || line.Contains($"csharp-doctor-disable: {ruleId}", StringComparison.OrdinalIgnoreCase);
    }

    private static bool LooksLikeNearMiss(string line) =>
        line.Contains("csharp-doctor-disable", StringComparison.OrdinalIgnoreCase)
        && !line.Contains(DisableLineMarker, StringComparison.OrdinalIgnoreCase)
        && !line.Contains(DisableNextLineMarker, StringComparison.OrdinalIgnoreCase);
}
