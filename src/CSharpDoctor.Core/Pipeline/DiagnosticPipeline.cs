namespace CSharpDoctor.Core.Pipeline;

using CSharpDoctor.Core.Diagnostics;

/// <summary>Applies ordered transforms to raw diagnostics.</summary>
public sealed class DiagnosticPipeline
{
    private readonly IReadOnlyList<IDiagnosticTransform> _transforms;

    public DiagnosticPipeline(IEnumerable<IDiagnosticTransform> transforms) =>
        _transforms = transforms.ToList();

    public IReadOnlyList<DiagnosticRecord> Apply(IEnumerable<DiagnosticRecord> diagnostics) =>
        diagnostics
            .Select(ApplySingle)
            .Where(static d => d is not null)
            .Cast<DiagnosticRecord>()
            .ToList();

    private DiagnosticRecord? ApplySingle(DiagnosticRecord diagnostic)
    {
        DiagnosticRecord? current = diagnostic;
        foreach (var transform in _transforms)
        {
            if (current is null)
            {
                return null;
            }

            current = transform.Apply(current);
        }

        return current;
    }
}
