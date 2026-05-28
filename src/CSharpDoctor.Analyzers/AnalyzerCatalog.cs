using System.Collections.Immutable;
using System.Reflection;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpDoctor.Analyzers;

/// <summary>Discovers all CSharp Doctor analyzers in this assembly.</summary>
public static class AnalyzerCatalog
{
    public static ImmutableArray<DiagnosticAnalyzer> CreateAll() =>
        typeof(AnalyzerCatalog).Assembly
            .GetTypes()
            .Where(t => !t.IsAbstract && typeof(DiagnosticAnalyzer).IsAssignableFrom(t))
            .Select(t => (DiagnosticAnalyzer)Activator.CreateInstance(t)!)
            .ToImmutableArray();
}
