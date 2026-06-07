namespace CSharpDoctor.Cli;

/// <summary>Tri-state result of resolving inspector check flags from the command line.</summary>
public sealed record CliCheckResult(
    bool? Lint,
    bool? Wiring,
    bool? Ef,
    bool? Architecture,
    bool? Sca,
    IReadOnlyList<string> Warnings,
    string? ConflictError);

/// <summary>
/// Pure resolution of check flags into tri-state overrides plus conflict/warning messages.
/// </summary>
public static class CliCheckResolver
{
    public static CliCheckResult Resolve(
        bool noLint,
        bool wiring,
        bool noWiring,
        bool ef,
        bool noEf,
        bool architecture,
        bool noArchitecture,
        bool sca,
        bool noSca,
        bool allChecks,
        bool staged)
    {
        if (wiring && noWiring)
        {
            return Conflict("Cannot combine --wiring and --no-wiring.");
        }

        if (ef && noEf)
        {
            return Conflict("Cannot combine --ef and --no-ef.");
        }

        if (architecture && noArchitecture)
        {
            return Conflict("Cannot combine --architecture and --no-architecture.");
        }

        if (sca && noSca)
        {
            return Conflict("Cannot combine --sca and --no-sca.");
        }

        var lint = noLint ? false : allChecks ? true : (bool?)null;
        var wiringResolved = noWiring ? false : wiring || allChecks ? true : (bool?)null;
        var efResolved = noEf ? false : ef || allChecks ? true : (bool?)null;
        var architectureResolved = noArchitecture ? false : architecture || allChecks ? true : (bool?)null;
        var scaResolved = noSca ? false : sca || allChecks ? true : (bool?)null;

        var warnings = new List<string>();
        if (staged && wiringResolved == true)
        {
            warnings.Add("--wiring needs whole-solution context; ignoring with --staged.");
            wiringResolved = false;
        }

        if (staged && architectureResolved == true)
        {
            warnings.Add("--architecture needs whole-solution context; ignoring with --staged.");
            architectureResolved = false;
        }

        return new CliCheckResult(lint, wiringResolved, efResolved, architectureResolved, scaResolved, warnings, ConflictError: null);
    }

    private static CliCheckResult Conflict(string message) =>
        new(Lint: null, Wiring: null, Ef: null, Architecture: null, Sca: null, Warnings: [], ConflictError: message);
}
