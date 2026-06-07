using CSharpDoctor.Core.Reporting;
using Shouldly;
using Xunit;

namespace CSharpDoctor.Core.UnitTests.Reporting;

public sealed class BaselineLoaderTests : IDisposable
{
    private readonly string _root;

    public BaselineLoaderTests()
    {
        _root = Path.Combine(Path.GetTempPath(), "csd-baseline-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_root);
    }

    [Fact]
    public void LoadIdentities_ReturnsNull_WhenFileMissing()
    {
        BaselineLoader.LoadIdentities(Path.Combine(_root, "missing.json")).ShouldBeNull();
    }

    [Fact]
    public void LoadIdentities_ParsesDiagnosticIdentities()
    {
        var path = Path.Combine(_root, "baseline.json");
        File.WriteAllText(
            path,
            """
            {
              "diagnostics": [
                { "identity": "a.cs::1:1::csharp-doctor/CSD0001" }
              ]
            }
            """);

        var identities = BaselineLoader.LoadIdentities(path);

        identities.ShouldNotBeNull();
        identities!.ShouldContain("a.cs::1:1::csharp-doctor/CSD0001");
    }

    public void Dispose()
    {
        try
        {
            Directory.Delete(_root, recursive: true);
        }
        catch
        {
            // best-effort cleanup
        }
    }
}
