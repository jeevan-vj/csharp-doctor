namespace CSharpDoctor.Core.Configuration;

/// <summary>Output channel that may apply different diagnostic filters.</summary>
public enum OutputSurface
{
    Cli,
    PrComment,
    Score,
    CiFailure,
}
