using CSharpDoctor.Core.Configuration;
using CSharpDoctor.Core.Diagnostics;
using CSharpDoctor.Core.Inspection;
using Shouldly;
using Xunit;

namespace CSharpDoctor.Core.UnitTests.Inspection;

public sealed class InspectorRegistryTests
{
    [Fact]
    public void Resolve_WithDefaultConfig_ReturnsOnlyLintInspector()
    {
        var config = new CSharpDoctorConfig();

        var inspectors = InspectorRegistry.Resolve(config, () => Fake("lint"));

        inspectors.Select(static i => i.Name).ShouldBe(["lint"]);
    }

    [Fact]
    public void Resolve_WithWiringEnabled_IncludesWiringInspector()
    {
        var config = new CSharpDoctorConfig { Checks = new ChecksConfig { Wiring = true } };
        var externals = new Dictionary<string, Func<IInspector>>
        {
            ["wiring"] = () => Fake("wiring"),
        };

        var inspectors = InspectorRegistry.Resolve(config, () => Fake("lint"), externals);

        inspectors.Select(static i => i.Name).ShouldBe(["lint", "wiring"]);
    }

    [Fact]
    public void Resolve_WithEfEnabled_IncludesEfInspector()
    {
        var config = new CSharpDoctorConfig { Checks = new ChecksConfig { Ef = true } };
        var externals = new Dictionary<string, Func<IInspector>>
        {
            ["ef"] = () => Fake("ef"),
        };

        var inspectors = InspectorRegistry.Resolve(config, () => Fake("lint"), externals);

        inspectors.Select(static i => i.Name).ShouldBe(["lint", "ef"]);
    }

    [Fact]
    public void Resolve_WithAllChecksOff_ReturnsEmpty()
    {
        var config = new CSharpDoctorConfig
        {
            Checks = new ChecksConfig { Lint = false, Wiring = false, Ef = false, Architecture = false, Sca = false },
        };

        var inspectors = InspectorRegistry.Resolve(config, () => Fake("lint"));

        inspectors.ShouldBeEmpty();
    }

    [Fact]
    public void Resolve_WithWiringEnabledButNoFactory_SkipsWiring()
    {
        var config = new CSharpDoctorConfig { Checks = new ChecksConfig { Wiring = true } };

        var inspectors = InspectorRegistry.Resolve(config, () => Fake("lint"));

        inspectors.Select(static i => i.Name).ShouldBe(["lint"]);
    }

    private static IInspector Fake(string name) => new FakeInspector(name);

    private sealed class FakeInspector(string name) : IInspector
    {
        public string Name => name;

        public Task<IReadOnlyList<DiagnosticRecord>> InspectAsync(
            InspectContext context,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<DiagnosticRecord>>([]);
    }
}
