using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.UnitTests.Identity.ValueObjects;

public sealed class FirstNameTests
{
    [Fact]
    public void Create_WithValidValue_ReturnsFirstName()
    {
        var result = FirstName.Create("John");

        Assert.Equal("John", result.Value);
    }

    [Fact]
    public void Create_WithWhitespace_TrimsValue()
    {
        var result = FirstName.Create("  John  ");

        Assert.Equal("John", result.Value);
    }

    [Fact]
    public void Create_WithNull_ThrowsInValidValueException()
    {
        var exception = Assert.Throws<InValidValueException>(
            () => FirstName.Create(null!));

        Assert.Equal(FirstName.RequiredMessage, exception.Message);
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
            () => FirstName.Create(value));

        Assert.Equal(FirstName.RequiredMessage, exception.Message);
    }

    [Fact]
    public void Create_WithMoreThan100Characters_ThrowsInValidValueException()
    {
        var value = new string('A', 101);

        var exception = Assert.Throws<InValidValueException>(
            () => FirstName.Create(value));

        Assert.Equal(FirstName.MaxLengthMessage, exception.Message);
    }

    [Fact]
    public void Create_WithExactly100Characters_ReturnsFirstName()
    {
        var value = new string('A', 100);

        var result = FirstName.Create(value);

        Assert.Equal(value, result.Value);
    }

    [Fact]
    public void TryCreate_WithValidValue_ReturnsFirstNameWithoutError()
    {
        var (firstName, error) =
            FirstName.TryCreate("John");

        Assert.NotNull(firstName);
        Assert.Null(error);
        Assert.Equal("John", firstName.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void TryCreate_WithEmptyValue_ReturnsRequiredError(
        string value)
    {
        var (firstName, error) =
            FirstName.TryCreate(value);

        Assert.Null(firstName);
        Assert.Equal(FirstName.RequiredMessage, error);
    }

    [Fact]
    public void TryCreate_WithMoreThan100Characters_ReturnsMaxLengthError()
    {
        var value = new string('A', 101);

        var (firstName, error) =
            FirstName.TryCreate(value);

        Assert.Null(firstName);
        Assert.Equal(FirstName.MaxLengthMessage, error);
    }

    [Fact]
    public void ToString_ReturnsValue()
    {
        var firstName = FirstName.Create("John");

        Assert.Equal("John", firstName.ToString());
    }
}