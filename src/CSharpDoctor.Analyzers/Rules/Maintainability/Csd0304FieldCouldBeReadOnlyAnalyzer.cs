using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpDoctor.Analyzers.Rules.Maintainability;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Csd0304FieldCouldBeReadOnlyAnalyzer : CSharpDoctorAnalyzerBase
{
    protected override RuleDescriptor Descriptor { get; } = new RuleDescriptor(
        "CSD0304",
        "Field could be readonly",
        "Maintainability",
        DiagnosticSeverity.Info,
        "Instance field '{0}' could be marked readonly",
        "Mark fields readonly when they are only assigned in constructors to express immutability intent.",
        new[] { "design", "maintainability" });

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(Analyze, SymbolKind.NamedType);
    }

    private void Analyze(SymbolAnalysisContext context)
    {
        if (context.Symbol is not INamedTypeSymbol type)
        {
            return;
        }

        foreach (var field in type.GetMembers().OfType<IFieldSymbol>())
        {
            if (field.IsStatic || field.IsConst || field.IsReadOnly)
            {
                continue;
            }

            if (field.DeclaredAccessibility is not (Accessibility.Private or Accessibility.Protected or Accessibility.Internal))
            {
                continue;
            }

            if (IsOnlyAssignedInConstructors(field))
            {
                var location = field.Locations.FirstOrDefault();
                if (location is not null)
                {
                    Report(context, location, field.Name);
                }
            }
        }
    }

    private static bool IsOnlyAssignedInConstructors(IFieldSymbol field)
    {
        return !field.ContainingType.GetMembers()
            .OfType<IMethodSymbol>()
            .Where(m => m.MethodKind is MethodKind.Constructor or MethodKind.StaticConstructor)
            .SelectMany(m => m.DeclaringSyntaxReferences)
            .Select(r => r.GetSyntax())
            .OfType<ConstructorDeclarationSyntax>()
            .SelectMany(c => c.DescendantNodes().OfType<AssignmentExpressionSyntax>())
            .Any(a => a.Left.ToString() == field.Name
                || a.Left.ToString().EndsWith("." + field.Name, StringComparison.Ordinal));
    }
}
