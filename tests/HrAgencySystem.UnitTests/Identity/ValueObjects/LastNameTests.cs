using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.UnitTests.Identity.ValueObjects;

public sealed class LastNameTests
{
    [Fact]
    public void Create_WithValidValue_ReturnsLastName()
    {
        var result = LastName.Create("Doe");

        Assert.Equal("Doe", result.Value);
    }

    [Fact]
    public void Create_WithWhitespace_TrimsValue()
    {
        var result = LastName.Create("  Doe  ");

        Assert.Equal("Doe", result.Value);
    }

    [Fact]
    public void Create_WithNull_ThrowsInValidValueException()
    {
        var exception = Assert.Throws<InValidValueException>(
            () => LastName.Create(null!));

        Assert.Equal(LastName.RequiredMessage, exception.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void Create_WithEmptyOrWhitespaceValue_ThrowsInValidValueException(
        string value)
    {
        var exception = Assert.Throws<InValidValueException>(
            () => LastName.Create(value));

        Assert.Equal(LastName.RequiredMessage, exception.Message);
    }

    [Fact]
    public void Create_WithMoreThan100Characters_ThrowsInValidValueException()
    {
        var value = new string('A', 101);

        var exception = Assert.Throws<InValidValueException>(
            () => LastName.Create(value));

        Assert.Equal(LastName.MaxLengthMessage, exception.Message);
    }

    [Fact]
    public void Create_WithExactly100Characters_ReturnsLastName()
    {
        var value = new string('A', 100);

        var result = LastName.Create(value);

        Assert.Equal(value, result.Value);
    }

    [Fact]
    public void TryCreate_WithValidValue_ReturnsLastNameWithoutError()
    {
        var (lastName, error) =
            LastName.TryCreate("Doe");

        Assert.NotNull(lastName);
        Assert.Null(error);
        Assert.Equal("Doe", lastName.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void TryCreate_WithEmptyValue_ReturnsRequiredError(
        string value)
    {
        var (lastName, error) =
            LastName.TryCreate(value);

        Assert.Null(lastName);
        Assert.Equal(LastName.RequiredMessage, error);
    }

    [Fact]
    public void TryCreate_WithMoreThan100Characters_ReturnsMaxLengthError()
    {
        var value = new string('A', 101);

        var (lastName, error) =
            LastName.TryCreate(value);

        Assert.Null(lastName);
        Assert.Equal(LastName.MaxLengthMessage, error);
    }

    [Fact]
    public void ToString_ReturnsValue()
    {
        var lastName = LastName.Create("Doe");

        Assert.Equal("Doe", lastName.ToString());
    }
}