using System.Collections.Immutable;
using CSharpDoctor.Core.Analysis;
using CSharpDoctor.Core.Diagnostics;
using CSharpDoctor.Core.Inspection;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpDoctor.Core.Inspectors;

/// <summary>Runs the bundled Roslyn analyzers over the in-scope source files.</summary>
public sealed class RoslynLintInspector : IInspector
{
    private readonly ImmutableArray<DiagnosticAnalyzer> _analyzers;

    public RoslynLintInspector(ImmutableArray<DiagnosticAnalyzer> analyzers) => _analyzers = analyzers;

    public string Name => "lint";

    public Task<IReadOnlyList<DiagnosticRecord>> InspectAsync(
        InspectContext context,
        CancellationToken cancellationToken = default) =>
        AnalyzerRunner.RunAsync(context.SourceFiles, _analyzers, cancellationToken);
}
