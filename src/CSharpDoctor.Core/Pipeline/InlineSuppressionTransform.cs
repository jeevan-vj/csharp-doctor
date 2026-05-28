namespace CSharpDoctor.Core.Pipeline;

using CSharpDoctor.Core.Configuration;
using CSharpDoctor.Core.Diagnostics;
using CSharpDoctor.Core.Suppression;

/// <summary>Removes diagnostics suppressed by inline comments.</summary>
public sealed class InlineSuppressionTransform : IDiagnosticTransform
{
    private readonly bool _respectInlineDisables;
    private readonly Func<string, IReadOnlyList<string>> _readFileLines;

    public InlineSuppressionTransform(
        CSharpDoctorConfig config,
        Func<string, IReadOnlyList<string>> readFileLines)
    {
        _respectInlineDisables = config.RespectInlineDisables;
        _readFileLines = readFileLines;
    }

    public DiagnosticRecord? Apply(DiagnosticRecord diagnostic)
    {
        if (!_respectInlineDisables)
        {
            return diagnostic;
        }

        var lines = _readFileLines(diagnostic.FilePath);
        var ruleId = $"{diagnostic.Plugin}/{diagnostic.Rule}";
        var result = SuppressionEvaluator.Evaluate(lines, diagnostic.Line, ruleId);

        if (result.IsSuppressed)
        {
            return null;
        }

        if (result.NearMissHint is not null)
        {
            return diagnostic with { SuppressionHint = result.NearMissHint };
        }

        return diagnostic;
    }
}
