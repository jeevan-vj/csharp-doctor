using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpDoctor.Analyzers;

/// <summary>Base type for CSharp Doctor diagnostic analyzers.</summary>
public abstract class CSharpDoctorAnalyzerBase : DiagnosticAnalyzer
{
    protected abstract RuleDescriptor Descriptor { get; }

    public sealed override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Descriptor.ToDescriptor());

    protected void Report(
        SyntaxNodeAnalysisContext context,
        Location location,
        params object?[] messageArgs) =>
        context.ReportDiagnostic(
            Diagnostic.Create(Descriptor.ToDescriptor(), location, messageArgs));

    protected void Report(
        SymbolAnalysisContext context,
        Location location,
        params object?[] messageArgs) =>
        context.ReportDiagnostic(
            Diagnostic.Create(Descriptor.ToDescriptor(), location, messageArgs));
}
