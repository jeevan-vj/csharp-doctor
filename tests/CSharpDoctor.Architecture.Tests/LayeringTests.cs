using NetArchTest.Rules;
using Shouldly;
using Xunit;

namespace CSharpDoctor.Architecture.Tests;

public sealed class LayeringTests
{
    [Fact]
    public void Analyzers_ShouldNotReference_Cli()
    {
        var result = Types.InAssembly(typeof(CSharpDoctor.Analyzers.AnalyzerCatalog).Assembly)
            .ShouldNot()
            .HaveDependencyOn("CSharpDoctor.Cli")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(result.ToString());
    }

    [Fact]
    public void Core_ShouldNotReference_CliOrCommandLine()
    {
        var result = Types.InAssembly(typeof(CSharpDoctor.Core.Diagnostics.DiagnosticRecord).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "CSharpDoctor.Cli",
                "System.CommandLine")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(result.ToString());
    }

    [Fact]
    public void Cli_MayReference_CoreAndAnalyzers()
    {
        var cliAssembly = typeof(CSharpDoctor.Cli.CliEntry).Assembly;
        cliAssembly.GetReferencedAssemblies()
            .Select(a => a.Name)
            .ShouldContain("CSharpDoctor.Core");
        cliAssembly.GetReferencedAssemblies()
            .Select(a => a.Name)
            .ShouldContain("CSharpDoctor.Analyzers");
    }
}
