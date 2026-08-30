using HrAgencySystem.Company.Domain.ValueObjects;

namespace HrAgencySystem.UnitTests.Company.ValueObjects;

public class CompanyNameTests
{
    [Theory]
    [InlineData("ACME")]
    [InlineData("Acme Corporation")]
    [InlineData("  ACME  ")]
    [InlineData("A")]
    public void Create_withValidValue_returnsCompanyName(string value)
    {
        var result = CompanyName.Create(value);

        Assert.Equal(result.Value, value.Trim());
    }

    [Fact]
    public void Create_WithWhitespaceOnly_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => CompanyName.Create("   "));
    }

    [Fact]
    public void Create_WithEmptyValue_ThrowsArgumentException()
    {
        var exception = Assert
            .Throws<ArgumentException>(() => CompanyName.Create(string.Empty));
        
        Assert.Contains(CompanyName.RequiredMessage, exception.Message);
    }

    [Fact]
    public void Create_WithValueLongerThan250Characters_ThrowsArgumentException()
    {
        var value = new string('A', 251);

        var exception = Assert
            .Throws<ArgumentException>(() => CompanyName.Create(value));
        
        Assert.Contains(CompanyName.MaxLengthMessage, exception.Message);
    }

    [Fact]
    public void Create_WithExactly250Characters_ReturnsCompanyName()
    {
        var value = new string('A', 250);

        var result = CompanyName.Create(value);

        Assert.Equal(result.Value, value);
        Assert.Equal(250, result.Value.Length);
    }
}