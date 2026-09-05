using HrAgencySystem.Organization.Domain.ValueObjects;
using HrAgencySystem.SharedKernel.Exception;

namespace HrAgencySystem.UnitTests.Organizations.ValueObjects;

public sealed class OrganizationNameTests
{
    [Fact]
    public void Create_WithValidValue_ReturnsOrganizationName()
    {
        // Arrange
        const string value = "HR Agency";

        // Act
        var result = OrganizationName.Create(value);

        // Assert
        Assert.Equal(value, result.Value);
    }

    [Fact]
    public void Create_WithLeadingAndTrailingWhitespace_TrimsValue()
    {
        // Arrange
        const string value = "  HR Agency  ";

        // Act
        var result = OrganizationName.Create(value);

        // Assert
        Assert.Equal("HR Agency", result.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void Create_WithEmptyOrWhitespaceValue_ThrowsInValidValueException(string value)
    {
        // Act
        var exception = Assert.Throws<InValidValueException>(
            () => OrganizationName.Create(value));

        // Assert
        Assert.Contains(OrganizationName.RequiredMessage, exception.Message);
    }

    [Fact]
    public void Create_WithMoreThan250Characters_ThrowsInValidValueException()
    {
        // Arrange
        var value = new string('A', 251);

        // Act
        var exception = Assert.Throws<InValidValueException>(
            () => OrganizationName.Create(value));

        // Assert
        Assert.Contains(OrganizationName.MaxLengthMessage, exception.Message);
    }

    [Fact]
    public void Create_WithExactly250Characters_ReturnsOrganizationName()
    {
        // Arrange
        var value = new string('A', 250);

        // Act
        var result = OrganizationName.Create(value);

        // Assert
        Assert.Equal(value, result.Value);
    }

    [Fact]
    public void TryCreate_WithValidValue_ReturnsOrganizationNameWithoutError()
    {
        // Arrange
        const string value = "HR Agency";

        // Act
        var (name, error) = OrganizationName.TryCreate(value);

        // Assert
        Assert.NotNull(name);
        Assert.Null(error);
        Assert.Equal(value, name.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void TryCreate_WithEmptyOrWhitespaceValue_ReturnsRequiredError(
        string value)
    {
        // Act
        var (name, error) = OrganizationName.TryCreate(value);

        // Assert
        Assert.Null(name);
        Assert.Equal(OrganizationName.RequiredMessage, error);
    }

    [Fact]
    public void TryCreate_WithMoreThan250Characters_ReturnsMaxLengthError()
    {
        // Arrange
        var value = new string('A', 251);

        // Act
        var (name, error) = OrganizationName.TryCreate(value);

        // Assert
        Assert.Null(name);
        Assert.Equal(OrganizationName.MaxLengthMessage, error);
    }

    [Fact]
    public void TryCreate_WithExactly250Characters_ReturnsOrganizationNameWithoutError()
    {
        // Arrange
        var value = new string('A', 250);

        // Act
        var (name, error) = OrganizationName.TryCreate(value);

        // Assert
        Assert.NotNull(name);
        Assert.Null(error);
        Assert.Equal(value, name.Value);
    }

    [Fact]
    public void TryCreate_WithLeadingAndTrailingWhitespace_TrimsValue()
    {
        // Arrange
        const string value = "  HR Agency  ";

        // Act
        var (name, error) = OrganizationName.TryCreate(value);

        // Assert
        Assert.NotNull(name);
        Assert.Null(error);
        Assert.Equal("HR Agency", name.Value);
    }

    [Fact]
    public void ToString_ReturnsValue()
    {
        // Arrange
        const string value = "HR Agency";
        var name = OrganizationName.Create(value);

        // Act
        var result = name.ToString();

        // Assert
        Assert.Equal(value, result);
    }

    [Fact]
    public void EqualValues_AreEqual()
    {
        // Arrange
        var first = OrganizationName.Create("HR Agency");
        var second = OrganizationName.Create("HR Agency");

        // Assert
        Assert.Equal(first, second);
    }

    [Fact]
    public void DifferentValues_AreNotEqual()
    {
        // Arrange
        var first = OrganizationName.Create("HR Agency");
        var second = OrganizationName.Create("Another Agency");

        // Assert
        Assert.NotEqual(first, second);
    }
}

