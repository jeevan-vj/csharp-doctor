using Microsoft.CodeAnalysis;

namespace CSharpDoctor.Wiring.Registration;

/// <summary>A constructor parameter on a type that participates in DI.</summary>
public sealed record ConstructorDependency(
    string ContainingTypeName,
    string ParameterTypeName,
    bool IsInterfaceOrAbstract,
    Location Location);
