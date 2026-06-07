using CSharpDoctor.Architecture.Graph;
using CSharpDoctor.Architecture.Rules;
using CSharpDoctor.Core.Configuration;
using Shouldly;
using Xunit;

namespace CSharpDoctor.Architecture.UnitTests.Rules;

public sealed class Csda0010Tests
{
    [Fact]
    public void Run_EmitsDiagnostic_WhenForbiddenNamespaceEdgeExists()
    {
        var graph = new NamespaceDependencyGraph();
        graph.AddEdge("Acme.UI.Controllers", "Acme.Data.Repositories");

        var policy = new ArchitecturePolicyConfig
        {
            ForbiddenNamespaceDependencies =
            [
                new ForbiddenDependencyRule(From: "Acme.UI", To: "Acme.Data"),
            ],
        };

        var diagnostics = Csda0010ForbiddenNamespaceDependency.Run(graph, policy).ToList();

        diagnostics.ShouldHaveSingleItem();
        diagnostics[0].Rule.ShouldBe("CSDA0010");
        diagnostics[0].Message.ShouldContain("Acme.UI");
        diagnostics[0].Message.ShouldContain("Acme.Data");
    }

    [Fact]
    public void Run_DoesNotEmit_WhenNoMatchingEdge()
    {
        var graph = new NamespaceDependencyGraph();
        graph.AddEdge("Acme.UI.Controllers", "Acme.Contracts");

        var policy = new ArchitecturePolicyConfig
        {
            ForbiddenNamespaceDependencies =
            [
                new ForbiddenDependencyRule(From: "Acme.UI", To: "Acme.Data"),
            ],
        };

        Csda0010ForbiddenNamespaceDependency.Run(graph, policy).ShouldBeEmpty();
    }

    [Fact]
    public void Run_DoesNotEmit_WhenPolicyEmpty()
    {
        var graph = new NamespaceDependencyGraph();
        graph.AddEdge("Acme.UI", "Acme.Data");

        Csda0010ForbiddenNamespaceDependency.Run(graph, new ArchitecturePolicyConfig()).ShouldBeEmpty();
    }
}
