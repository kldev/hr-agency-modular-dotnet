using HrAgencySystem.Identity.Application.Policy;
using HrAgencySystem.SharedKernel.Exception;

namespace HrAgencySystem.UnitTests.Identity.Policy;

public sealed class AvatarUploadPolicyTests
{
    [Theory]
    [InlineData("image/png")]
    [InlineData("image/jpeg")]
    [InlineData("IMAGE/PNG")]
    // A browser is entitled to spell out the charset; it is still the same type.
    [InlineData("image/png; charset=binary")]
    public void Validate_WithAnAcceptedImage_Passes(string contentType)
    {
        AvatarUploadPolicy.Validate(contentType, 1024);
    }

    [Fact]
    public void Validate_WithExactlyTheMaximumSize_Passes()
    {
        AvatarUploadPolicy.Validate("image/png", AvatarUploadPolicy.MaxSizeBytes);
    }

    [Fact]
    public void Validate_WithOneByteOverTheMaximum_Throws()
    {
        var exception = Assert.Throws<ValidationException>(() =>
            AvatarUploadPolicy.Validate("image/png", AvatarUploadPolicy.MaxSizeBytes + 1)
        );

        Assert.Contains(AvatarUploadPolicy.TooLargeMessage, exception.Errors);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WithNoBytes_Throws(long size)
    {
        var exception = Assert.Throws<ValidationException>(() =>
            AvatarUploadPolicy.Validate("image/png", size)
        );

        Assert.Contains(AvatarUploadPolicy.EmptyFileMessage, exception.Errors);
    }

    [Theory]
    [InlineData("application/pdf")]
    [InlineData("image/gif")]
    // Accepted by the file service for documents, still not a profile picture.
    [InlineData("image/webp")]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_WithAnythingButPngOrJpeg_Throws(string? contentType)
    {
        var exception = Assert.Throws<ValidationException>(() =>
            AvatarUploadPolicy.Validate(contentType, 1024)
        );

        Assert.Contains(AvatarUploadPolicy.UnsupportedTypeMessage, exception.Errors);
    }

    /// <summary>
    /// Both faults are reported together, the way every other validation in the codebase reports
    /// them - a person fixing one should not have to upload again to discover the other.
    /// </summary>
    [Fact]
    public void Validate_WithBothFaults_ReportsBoth()
    {
        var exception = Assert.Throws<ValidationException>(() =>
            AvatarUploadPolicy.Validate("application/pdf", AvatarUploadPolicy.MaxSizeBytes + 1)
        );

        Assert.Contains(AvatarUploadPolicy.TooLargeMessage, exception.Errors);
        Assert.Contains(AvatarUploadPolicy.UnsupportedTypeMessage, exception.Errors);
    }
}
