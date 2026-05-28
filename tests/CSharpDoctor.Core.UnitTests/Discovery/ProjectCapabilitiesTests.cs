using CSharpDoctor.Core.Discovery;
using Shouldly;
using Xunit;

namespace CSharpDoctor.Core.UnitTests.Discovery;

public sealed class ProjectCapabilitiesTests
{
    [Fact]
    public void FromCsproj_DetectsAspNetCore_WhenWebSdkUsed()
    {
        var path = Path.Combine(Path.GetTempPath(), $"csd-csproj-{Guid.NewGuid():N}.csproj");
        File.WriteAllText(
            path,
            """
            <Project Sdk="Microsoft.NET.Sdk.Web">
              <PropertyGroup><TargetFramework>net10.0</TargetFramework></PropertyGroup>
            </Project>
            """);

        try
        {
            var caps = ProjectCapabilities.FromCsproj(path);
            caps.AspNetCore.ShouldBeTrue();
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void FromCsproj_ReturnsNone_WhenPlainClassLibrary()
    {
        var path = Path.Combine(Path.GetTempPath(), $"csd-csproj-{Guid.NewGuid():N}.csproj");
        File.WriteAllText(
            path,
            """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup><TargetFramework>net10.0</TargetFramework></PropertyGroup>
            </Project>
            """);

        try
        {
            var caps = ProjectCapabilities.FromCsproj(path);
            caps.AspNetCore.ShouldBeFalse();
            caps.EntityFrameworkCore.ShouldBeFalse();
        }
        finally
        {
            File.Delete(path);
        }
    }
}
