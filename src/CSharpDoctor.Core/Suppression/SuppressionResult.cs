namespace CSharpDoctor.Core.Suppression;

/// <summary>Result of evaluating inline suppression comments.</summary>
public sealed record SuppressionResult(bool IsSuppressed, string? NearMissHint = null);
