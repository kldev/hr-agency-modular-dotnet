using HrAgencySystem.Company.Domain.ValueObjects;

namespace HrAgencySystem.UnitTests.Company.ValueObjects;

public class TaxIdTests
{
    [Theory]
    [InlineData("PL909-999-111")]
    [InlineData("DE123455555")]
    [InlineData("DE/123/1234")]
    [InlineData("  TAX-123  ")]
    public void Create_WithValidValue_ReturnsTaxId(string value)
    {
        var result = TaxId.Create(value);

        Assert.Equal(value.Trim(), result.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Create_WithMissingValue_ThrowsArgumentException(string value)
    {
        Assert
            .Throws<ArgumentException>(() => TaxId.Create(value));
    }

    [Fact]
    public void Create_WithMoreThan50Characters_ThrowsArgumentException()
    {
        var value = new string('A', 51);

        Assert
            .Throws<ArgumentException>(() => TaxId.Create(value));
    }

    [Fact]
    public void Create_WithExactly50Characters_ReturnsTaxId()
    {
        var value = new string('A', 50);

        var result = TaxId.Create(value);

        Assert.Equal(value, result.Value);
        Assert.Equal(50, result.Value.Length);
    }
}