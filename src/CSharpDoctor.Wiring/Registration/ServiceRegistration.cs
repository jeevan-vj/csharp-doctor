using Microsoft.CodeAnalysis;

namespace CSharpDoctor.Wiring.Registration;

/// <summary>A single service registration discovered in composition roots.</summary>
public sealed record ServiceRegistration(
    string ServiceTypeName,
    string? ImplementationTypeName,
    ServiceLifetime Lifetime,
    Location Location);
