using HrAgencySystem.Organization.Domain.ValueObjects;
using HrAgencySystem.SharedKernel.Exception;

namespace HrAgencySystem.UnitTests.Organization.ValueObjects;

public sealed class OrganizationSlugTests
{
    [Fact]
    public void Create_WithValidValue_ReturnsOrganizationSlug()
    {
        // Arrange
        const string value = "hr-agency";

        // Act
        var result = OrganizationSlug.Create(value);

        // Assert
        Assert.Equal(value, result.Value);
    }

    [Fact]
    public void Create_WithUppercaseValue_NormalizesToLowercase()
    {
        // Arrange
        const string value = "HR-AGENCY";

        // Act
        var result = OrganizationSlug.Create(value);

        // Assert
        Assert.Equal("hr-agency", result.Value);
    }

    [Fact]
    public void Create_WithLeadingAndTrailingWhitespace_TrimsValue()
    {
        // Arrange
        const string value = "  hr-agency  ";

        // Act
        var result = OrganizationSlug.Create(value);

        // Assert
        Assert.Equal("hr-agency", result.Value);
    }

    [Fact]
    public void Create_WithMixedCaseAndWhitespace_NormalizesValue()
    {
        // Arrange
        const string value = "  Hr-AgEnCy  ";

        // Act
        var result = OrganizationSlug.Create(value);

        // Assert
        Assert.Equal("hr-agency", result.Value);
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
        // Act
        var exception = Assert.Throws<InValidValueException>(
            () => OrganizationSlug.Create(value));

        // Assert
        Assert.Contains(OrganizationSlug.RequiredMessage, exception.Message);
    }

    [Fact]
    public void Create_WithMoreThan100Characters_ThrowsInValidValueException()
    {
        // Arrange
        var value = new string('a', 101);

        // Act
        var exception = Assert.Throws<InValidValueException>(
            () => OrganizationSlug.Create(value));

        // Assert
        Assert.Contains(OrganizationSlug.MaxLengthMessage, exception.Message);
    }

    [Fact]
    public void Create_WithExactly100Characters_ReturnsOrganizationSlug()
    {
        // Arrange
        var value = new string('a', 100);

        // Act
        var result = OrganizationSlug.Create(value);

        // Assert
        Assert.Equal(value, result.Value);
    }

    [Fact]
    public void TryCreate_WithValidValue_ReturnsOrganizationSlugWithoutError()
    {
        // Arrange
        const string value = "hr-agency";

        // Act
        var (slug, error) = OrganizationSlug.TryCreate(value);

        // Assert
        Assert.NotNull(slug);
        Assert.Null(error);
        Assert.Equal(value, slug.Value);
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
        var (slug, error) = OrganizationSlug.TryCreate(value);

        // Assert
        Assert.Null(slug);
        Assert.Equal(OrganizationSlug.RequiredMessage, error);
    }

    [Fact]
    public void TryCreate_WithNullValue_ReturnsRequiredError()
    {
        // Act
        var (slug, error) = OrganizationSlug.TryCreate(null!);

        // Assert
        Assert.Null(slug);
        Assert.Equal(OrganizationSlug.RequiredMessage, error);
    }

    [Fact]
    public void Create_WithNullValue_ThrowsInValidValueException()
    {
        // Act
        var exception = Assert.Throws<InValidValueException>(
            () => OrganizationSlug.Create(null!));

        // Assert
        Assert.Contains(OrganizationSlug.RequiredMessage, exception.Message);
    }

    [Fact]
    public void TryCreate_WithMoreThan100Characters_ReturnsMaxLengthError()
    {
        // Arrange
        var value = new string('a', 101);

        // Act
        var (slug, error) = OrganizationSlug.TryCreate(value);

        // Assert
        Assert.Null(slug);
        Assert.Equal(OrganizationSlug.MaxLengthMessage, error);
    }

    [Fact]
    public void TryCreate_WithExactly100Characters_ReturnsOrganizationSlugWithoutError()
    {
        // Arrange
        var value = new string('a', 100);

        // Act
        var (slug, error) = OrganizationSlug.TryCreate(value);

        // Assert
        Assert.NotNull(slug);
        Assert.Null(error);
        Assert.Equal(value, slug.Value);
    }

    [Fact]
    public void TryCreate_WithUppercaseValue_NormalizesToLowercase()
    {
        // Arrange
        const string value = "HR-AGENCY";

        // Act
        var (slug, error) = OrganizationSlug.TryCreate(value);

        // Assert
        Assert.NotNull(slug);
        Assert.Null(error);
        Assert.Equal("hr-agency", slug.Value);
    }

    [Fact]
    public void TryCreate_WithLeadingAndTrailingWhitespace_TrimsValue()
    {
        // Arrange
        const string value = "  hr-agency  ";

        // Act
        var (slug, error) = OrganizationSlug.TryCreate(value);

        // Assert
        Assert.NotNull(slug);
        Assert.Null(error);
        Assert.Equal("hr-agency", slug.Value);
    }

    [Fact]
    public void ToString_ReturnsValue()
    {
        // Arrange
        const string value = "hr-agency";
        var slug = OrganizationSlug.Create(value);

        // Act
        var result = slug.ToString();

        // Assert
        Assert.Equal(value, result);
    }

    [Fact]
    public void EqualValues_AreEqual()
    {
        // Arrange
        var first = OrganizationSlug.Create("hr-agency");
        var second = OrganizationSlug.Create("hr-agency");

        // Assert
        Assert.Equal(first, second);
    }

    [Fact]
    public void DifferentValues_AreNotEqual()
    {
        // Arrange
        var first = OrganizationSlug.Create("hr-agency");
        var second = OrganizationSlug.Create("another-agency");

        // Assert
        Assert.NotEqual(first, second);
    }
}
