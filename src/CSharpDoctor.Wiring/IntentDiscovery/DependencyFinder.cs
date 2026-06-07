using CSharpDoctor.Wiring.Registration;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpDoctor.Wiring.IntentDiscovery;

/// <summary>Finds constructor dependencies on types that participate in DI.</summary>
public static class DependencyFinder
{
    public static IEnumerable<ConstructorDependency> Find(
        IEnumerable<SyntaxNode> roots,
        RegistrationGraph registrations)
    {
        var participatingTypes = CollectParticipatingTypeNames(roots, registrations);

        foreach (var root in roots)
        {
            foreach (var classDecl in root.DescendantNodes().OfType<ClassDeclarationSyntax>())
            {
                if (classDecl.Modifiers.Any(SyntaxKind.AbstractKeyword))
                {
                    continue;
                }

                var typeName = classDecl.Identifier.ValueText;
                if (!participatingTypes.Contains(typeName))
                {
                    continue;
                }

                foreach (var constructor in classDecl.Members.OfType<ConstructorDeclarationSyntax>())
                {
                    foreach (var parameter in constructor.ParameterList.Parameters)
                    {
                        var typeNameParam = parameter.Type?.ToString();
                        if (typeNameParam is null)
                        {
                            continue;
                        }

                        var simple = WiringTypeIdentity.Normalize(typeNameParam);
                        var isInterface = simple.Length > 1 && simple[0] == 'I' && char.IsUpper(simple[1]);

                        yield return new ConstructorDependency(
                            typeName,
                            typeNameParam,
                            IsInterfaceOrAbstract: isInterface,
                            parameter.GetLocation());
                    }
                }
            }
        }
    }

    private static HashSet<string> CollectParticipatingTypeNames(
        IEnumerable<SyntaxNode> roots,
        RegistrationGraph registrations)
    {
        var types = new HashSet<string>(StringComparer.Ordinal);

        foreach (var reg in registrations.Registrations)
        {
            types.Add(WiringTypeIdentity.Normalize(reg.ServiceTypeName));
            if (reg.ImplementationTypeName is not null)
            {
                types.Add(WiringTypeIdentity.Normalize(reg.ImplementationTypeName));
            }
        }

        foreach (var candidate in roots.SelectMany(HostedServiceFinder.Find))
        {
            types.Add(candidate.TypeName);
        }

        foreach (var root in roots)
        {
            foreach (var classDecl in root.DescendantNodes().OfType<ClassDeclarationSyntax>())
            {
                if (classDecl.BaseList?.Types.Any(t =>
                        t.Type.ToString().Contains("Controller", StringComparison.Ordinal)) == true)
                {
                    types.Add(classDecl.Identifier.ValueText);
                }
            }
        }

        return types;
    }
}
