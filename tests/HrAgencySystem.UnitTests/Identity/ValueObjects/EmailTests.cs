using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.UnitTests.Identity.ValueObjects;

public sealed class EmailTests
{
    [Fact]
    public void Create_WithValidEmail_ReturnsEmail()
    {
        var result = Email.Create("john.doe@example.com");

        Assert.Equal("john.doe@example.com", result.Value);
    }

    [Fact]
    public void Create_WithEmailContainingWhitespace_TrimsAndNormalizesEmail()
    {
        var result = Email.Create("  JOHN.DOE@EXAMPLE.COM  ");

        Assert.Equal("john.doe@example.com", result.Value);
    }

    [Fact]
    public void Create_WithNull_ThrowsInValidValueException()
    {
        var exception = Assert.Throws<InValidValueException>(
            () => Email.Create(null!));

        Assert.Equal(Email.RequiredMessage, exception.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Create_WithEmptyEmail_ThrowsInValidValueException(
        string value)
    {
        var exception = Assert.Throws<InValidValueException>(
            () => Email.Create(value));

        Assert.Equal(Email.RequiredMessage, exception.Message);
    }

    [Fact]
    public void Create_WithMoreThan320Characters_ThrowsInValidValueException()
    {
        var value = $"{new string('a', 312)}@example.com";

        Assert.Equal(324, value.Length);

        var exception = Assert.Throws<InValidValueException>(
            () => Email.Create(value));

        Assert.Equal(Email.MaxLengthMessage, exception.Message);
    }

    [Fact]
    public void Create_WithExactly320Characters_ReturnsEmail()
    {
        var localPartLength = 308;
        var value = $"{new string('a', localPartLength)}@example.com";

        Assert.Equal(320, value.Length);

        var result = Email.Create(value);

        Assert.Equal(value, result.Value);
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("invalid@")]
    [InlineData("@example.com")]
    [InlineData("invalid.example.com")]
    [InlineData("invalid @example.com")]
    public void Create_WithInvalidEmail_ThrowsInValidValueException(
        string value)
    {
        var exception = Assert.Throws<InValidValueException>(
            () => Email.Create(value));

        Assert.Equal(Email.InvalidEmail, exception.Message);
    }

    [Fact]
    public void TryCreate_WithValidEmail_ReturnsEmailWithoutError()
    {
        var (email, error) =
            Email.TryCreate("john.doe@example.com");

        Assert.NotNull(email);
        Assert.Null(error);
        Assert.Equal("john.doe@example.com", email.Value);
    }

    [Fact]
    public void TryCreate_WithInvalidEmail_ReturnsErrorWithoutThrowing()
    {
        var (email, error) =
            Email.TryCreate("invalid");

        Assert.Null(email);
        Assert.Equal(Email.InvalidEmail, error);
    }

    [Fact]
    public void ToString_ReturnsEmailValue()
    {
        var email = Email.Create("john.doe@example.com");

        Assert.Equal("john.doe@example.com", email.ToString());
    }
}