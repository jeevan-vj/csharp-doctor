using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpDoctor.Analyzers.Rules.Correctness;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Csd0107ClassDisposeAnalyzer : CSharpDoctorAnalyzerBase
{
    protected override RuleDescriptor Descriptor { get; } = new RuleDescriptor(
        "CSD0107",
        "IDisposable field not disposed in Dispose",
        "Correctness",
        DiagnosticSeverity.Warning,
        "Field '{0}' implements IDisposable but is not disposed in Dispose()",
        "Dispose owned IDisposable fields in the class Dispose method.",
        new[] { "correctness" });

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
    }

    private void AnalyzeNamedType(SymbolAnalysisContext context)
    {
        if (context.Symbol is not INamedTypeSymbol type
            || !ImplementsDisposable(type))
        {
            return;
        }

        var disposeMethod = type.GetMembers("Dispose")
            .OfType<IMethodSymbol>()
            .FirstOrDefault(m => m.Parameters.Length == 0);

        if (disposeMethod is null)
        {
            return;
        }

        foreach (var field in type.GetMembers().OfType<IFieldSymbol>())
        {
            if (!IsDisposableField(field))
            {
                continue;
            }

            if (!IsDisposedInMethod(disposeMethod, field.Name))
            {
                var location = field.Locations.FirstOrDefault() ?? type.Locations.First();
                Report(context, location, field.Name);
            }
        }
    }

    private static bool ImplementsDisposable(INamedTypeSymbol type) =>
        type.AllInterfaces.Any(i => i.Name == "IDisposable");

    private static bool IsDisposableField(IFieldSymbol field) =>
        field.Type.AllInterfaces.Any(i => i.Name == "IDisposable")
        || field.Type.Name is "FileStream" or "StreamReader" or "StreamWriter";

    private static bool IsDisposedInMethod(IMethodSymbol disposeMethod, string fieldName)
    {
        var syntax = disposeMethod.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax();
        if (syntax is null)
        {
            return false;
        }

        var text = syntax.ToString();
        return text.Contains($"{fieldName}.Dispose()", StringComparison.Ordinal)
            || text.Contains($"{fieldName}?.Dispose()", StringComparison.Ordinal);
    }
}
