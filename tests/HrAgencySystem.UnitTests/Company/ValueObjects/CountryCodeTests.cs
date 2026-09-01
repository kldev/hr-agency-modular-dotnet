using HrAgencySystem.Company.Domain.ValueObjects;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.UnitTests.Company.ValueObjects;

public class CountryCodeTests
{
    [Theory]
    [InlineData("PL")]
    [InlineData("DE")]
    [InlineData("FR")]
    [InlineData("pl")]
    [InlineData(" de ")]
    public void Create_WithValidValue_ReturnsNormalizedCountryCode(string value)
    {
        var result = CountryCode.Create(value);

        Assert.Equal(result.Value, value.Trim().ToUpperInvariant());
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Create_WithMissingValue_ThrowsInValidValueException(string value)
    {
        var exception = Assert
            .Throws<InValidValueException>(() => CountryCode.Create(value));
        Assert.Equal(CountryCode.RequiredMessage, exception.Message);
    }

    [Theory]
    [InlineData("P")]
    [InlineData("POL")]
    [InlineData("POLAND")]
    [InlineData("123")]
    public void Create_WithInvalidLength_ThrowsInValidValueException(string value)
    {
        var exception = Assert
            .Throws<InValidValueException>(() => CountryCode.Create(value));
        Assert.Equal(CountryCode.InvalidFormatMessage, exception.Message);
    }

    [Theory]
    [InlineData("P1")]
    [InlineData("1L")]
    [InlineData("12")]
    [InlineData("@#")]
    public void Create_WithNonLetterCharacters_ThrowsInValidValueException(string value)
    {
        var exception = Assert
            .Throws<InValidValueException>(() => CountryCode.Create(value));

        Assert.Equal(CountryCode.OnlyCharactersAllowedMessage, exception.Message);
    }

    [Fact]
    public void Create_WithExactlyTwoLetters_ReturnsCountryCode()
    {
        var result = CountryCode.Create("pl");

        Assert.Equal("PL", result.Value);
    }
}