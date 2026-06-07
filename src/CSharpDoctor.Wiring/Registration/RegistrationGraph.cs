using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpDoctor.Wiring.Registration;

/// <summary>
/// Records service registrations discovered in composition roots. Tracks lifetimes for
/// AddSingleton/AddScoped/AddTransient and hosted services.
/// </summary>
public sealed class RegistrationGraph
{
    private readonly List<ServiceRegistration> _registrations = [];
    private readonly HashSet<string> _registeredHostedServices = new(StringComparer.Ordinal);

    public IReadOnlyList<ServiceRegistration> Registrations => _registrations;

    public static RegistrationGraph Build(IEnumerable<SyntaxNode> roots)
    {
        var graph = new RegistrationGraph();
        foreach (var root in roots)
        {
            graph.Scan(root);
        }

        return graph;
    }

    public bool IsHostedServiceRegistered(string typeName) =>
        _registeredHostedServices.Contains(typeName);

    public bool IsServiceRegistered(string typeName)
    {
        var normalized = WiringTypeIdentity.Normalize(typeName);
        return _registrations.Any(r =>
            WiringTypeIdentity.Normalize(r.ServiceTypeName).Equals(normalized, StringComparison.Ordinal)
            || (r.ImplementationTypeName is not null
                && WiringTypeIdentity.Normalize(r.ImplementationTypeName).Equals(normalized, StringComparison.Ordinal)));
    }

    public ServiceLifetime? GetLifetime(string typeName)
    {
        var normalized = WiringTypeIdentity.Normalize(typeName);
        var match = _registrations.LastOrDefault(r =>
            WiringTypeIdentity.Normalize(r.ServiceTypeName).Equals(normalized, StringComparison.Ordinal)
            || (r.ImplementationTypeName is not null
                && WiringTypeIdentity.Normalize(r.ImplementationTypeName).Equals(normalized, StringComparison.Ordinal)));

        return match?.Lifetime;
    }

    private void Scan(SyntaxNode root)
    {
        foreach (var invocation in root.DescendantNodes().OfType<InvocationExpressionSyntax>())
        {
            if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
            {
                continue;
            }

            var methodName = memberAccess.Name.Identifier.ValueText;

            if (memberAccess.Name is GenericNameSyntax genericHosted
                && methodName == "AddHostedService"
                && genericHosted.TypeArgumentList.Arguments.Count == 1)
            {
                var registered = TypeArgumentName(genericHosted.TypeArgumentList.Arguments[0]);
                if (registered is not null)
                {
                    _registeredHostedServices.Add(registered);
                    _registrations.Add(new ServiceRegistration(
                        registered, registered, ServiceLifetime.Singleton, invocation.GetLocation()));
                }

                continue;
            }

            if (memberAccess.Name is not GenericNameSyntax generic || generic.TypeArgumentList.Arguments.Count == 0)
            {
                continue;
            }

            var lifetime = methodName switch
            {
                "AddSingleton" => ServiceLifetime.Singleton,
                "AddScoped" => ServiceLifetime.Scoped,
                "AddTransient" => ServiceLifetime.Transient,
                _ => (ServiceLifetime?)null,
            };

            if (lifetime is null)
            {
                continue;
            }

            var args = generic.TypeArgumentList.Arguments;
            var serviceType = TypeArgumentName(args[0]);
            var implType = args.Count > 1 ? TypeArgumentName(args[1]) : serviceType;
            if (serviceType is null)
            {
                continue;
            }

            _registrations.Add(new ServiceRegistration(
                serviceType,
                implType,
                lifetime.Value,
                invocation.GetLocation()));
        }
    }

    private static string? TypeArgumentName(TypeSyntax type) => type switch
    {
        IdentifierNameSyntax identifier => identifier.Identifier.ValueText,
        QualifiedNameSyntax qualified => qualified.Right.Identifier.ValueText,
        GenericNameSyntax generic => generic.Identifier.ValueText,
        _ => null,
    };
}
