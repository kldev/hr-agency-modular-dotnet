using HrAgencySystem.Company.Domain.ValueObjects;

namespace HrAgencySystem.UnitTests.Company.ValueObjects;

public class RegistrationNumberTests
{
    [Theory]
    [InlineData("REG-123")]
    [InlineData("123456")]
    [InlineData("ABC/123")]
    [InlineData("  REG-123  ")]
    public void Create_WithValidValue_ReturnsRegistrationNumber(string value)
    {
        var result = RegistrationNumber.Create(value);

        Assert.Equal(value.Trim(), result.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Create_WithMissingValue_ThrowsArgumentException(string value)
    {
        var exception = Assert
            .Throws<ArgumentException>(() => RegistrationNumber.Create(value));

        Assert.Contains(
            RegistrationNumber.RequiredMessage,
            exception.Message);
    }

    [Fact]
    public void Create_WithMoreThan100Characters_ThrowsArgumentException()
    {
        var value = new string('A', 101);

        var exception = Assert
            .Throws<ArgumentException>(() => RegistrationNumber.Create(value));

        Assert.Contains(
            RegistrationNumber.MaxLengthMessage,
            exception.Message);
    }

    [Fact]
    public void Create_WithExactly100Characters_ReturnsRegistrationNumber()
    {
        var value = new string('A', 100);

        var result = RegistrationNumber.Create(value);

        Assert.Contains(value, result.Value);
        Assert.Equal(100, result.Value.Length);
    }
}