namespace CSharpDoctor.Ef.Migrations;

/// <summary>Detects likely data-loss column type changes in migrations.</summary>
public static class TypeShrinkAnalyzer
{
    public static bool IsLikelyDataLoss(string? oldType, string? newType)
    {
        if (string.IsNullOrWhiteSpace(oldType) || string.IsNullOrWhiteSpace(newType))
        {
            return false;
        }

        var oldLen = ParseStringLength(oldType);
        var newLen = ParseStringLength(newType);
        if (oldLen is not null && newLen is not null && newLen < oldLen)
        {
            return true;
        }

        var oldDecimal = ParseDecimal(oldType);
        var newDecimal = ParseDecimal(newType);
        if (oldDecimal is not null && newDecimal is not null
            && (newDecimal.Value.precision < oldDecimal.Value.precision
                || newDecimal.Value.scale < oldDecimal.Value.scale))
        {
            return true;
        }

        if (oldType.Contains("datetime2", StringComparison.OrdinalIgnoreCase)
            && newType.Contains("datetime", StringComparison.OrdinalIgnoreCase)
            && !newType.Contains("datetime2", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return false;
    }

    private static int? ParseStringLength(string type)
    {
        var open = type.IndexOf('(');
        var close = type.IndexOf(')');
        if (open < 0 || close <= open)
        {
            return null;
        }

        var inner = type[(open + 1)..close];
        return int.TryParse(inner, out var len) ? len : null;
    }

    private static (int precision, int scale)? ParseDecimal(string type)
    {
        if (!type.Contains("decimal", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var open = type.IndexOf('(');
        var close = type.IndexOf(')');
        if (open < 0 || close <= open)
        {
            return null;
        }

        var parts = type[(open + 1)..close].Split(',');
        if (parts.Length != 2
            || !int.TryParse(parts[0].Trim(), out var precision)
            || !int.TryParse(parts[1].Trim(), out var scale))
        {
            return null;
        }

        return (precision, scale);
    }
}
