namespace CSharpDoctor.Wiring.Registration;

/// <summary>DI service lifetime as declared in the composition root.</summary>
public enum ServiceLifetime
{
    Singleton,
    Scoped,
    Transient,
}
