namespace CSharpDoctor.Core.Pipeline;

using CSharpDoctor.Core.Diagnostics;

/// <summary>Single step in the diagnostic post-processing pipeline.</summary>
public interface IDiagnosticTransform
{
    DiagnosticRecord? Apply(DiagnosticRecord diagnostic);
}
