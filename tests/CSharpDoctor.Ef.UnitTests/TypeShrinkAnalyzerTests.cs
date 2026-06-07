using CSharpDoctor.Ef.Migrations;
using Shouldly;
using Xunit;

namespace CSharpDoctor.Ef.UnitTests;

public sealed class TypeShrinkAnalyzerTests
{
    [Theory]
    [InlineData("nvarchar(500)", "nvarchar(100)", true)]
    [InlineData("varchar(200)", "varchar(50)", true)]
    [InlineData("nvarchar(100)", "nvarchar(500)", false)]
    [InlineData("decimal(18,4)", "decimal(10,2)", true)]
    [InlineData("datetime2", "datetime", true)]
    public void IsLikelyDataLoss_DetectsShrinks(string oldType, string newType, bool expected) =>
        TypeShrinkAnalyzer.IsLikelyDataLoss(oldType, newType).ShouldBe(expected);
}
