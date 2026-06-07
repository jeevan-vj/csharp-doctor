using CSharpDoctor.Architecture.Rules;
using CSharpDoctor.Core.Configuration;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;

namespace CSharpDoctor.Architecture.UnitTests.Rules;

public sealed class Csda0011Tests
{
    [Fact]
    public void Run_EmitsDiagnostic_WhenForbiddenAssemblyReferenceExists()
    {
        var solution = BuildSolution(fromName: "Acme.UI", toName: "Acme.Data", addReference: true);

        var policy = new ArchitecturePolicyConfig
        {
            ForbiddenAssemblyReferences =
            [
                new ForbiddenDependencyRule(From: "Acme.UI", To: "Acme.Data"),
            ],
        };

        var diagnostics = Csda0011ForbiddenAssemblyReference.Run(solution, policy).ToList();

        diagnostics.ShouldHaveSingleItem();
        diagnostics[0].Rule.ShouldBe("CSDA0011");
    }

    [Fact]
    public void Run_DoesNotEmit_WhenReferenceAllowed()
    {
        var solution = BuildSolution(fromName: "Acme.UI", toName: "Acme.Contracts", addReference: true);

        var policy = new ArchitecturePolicyConfig
        {
            ForbiddenAssemblyReferences =
            [
                new ForbiddenDependencyRule(From: "Acme.UI", To: "Acme.Data"),
            ],
        };

        Csda0011ForbiddenAssemblyReference.Run(solution, policy).ShouldBeEmpty();
    }

    private static Solution BuildSolution(string fromName, string toName, bool addReference)
    {
        using var workspace = new AdhocWorkspace();
        var toProjectId = ProjectId.CreateNewId();
        var fromProjectId = ProjectId.CreateNewId();

        var solution = workspace.CurrentSolution
            .AddProject(ProjectInfo.Create(
                toProjectId,
                VersionStamp.Create(),
                toName,
                toName,
                LanguageNames.CSharp))
            .AddProject(ProjectInfo.Create(
                fromProjectId,
                VersionStamp.Create(),
                fromName,
                fromName,
                LanguageNames.CSharp));

        if (addReference)
        {
            solution = solution.AddProjectReference(fromProjectId, new ProjectReference(toProjectId));
        }

        return solution;
    }
}
