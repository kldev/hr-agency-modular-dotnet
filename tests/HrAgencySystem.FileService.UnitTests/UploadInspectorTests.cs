using HrAgencySystem.FileService.Application;
using HrAgencySystem.FileService.Config;
using Microsoft.Extensions.Options;

namespace HrAgencySystem.FileService.UnitTests;

public sealed class UploadInspectorTests
{
    private const long MaxSize = 1024;

    private readonly UploadInspector _inspector = new(
        Options.Create(new FileServiceConfig { MaxSizeBytes = MaxSize })
    );

    [Fact]
    public void Inspect_AcceptsAnAllowedType()
    {
        Assert.Null(_inspector.Inspect("application/pdf", "umowa.pdf", 512));
    }

    [Fact]
    public void Inspect_AcceptsATypeCarryingACharset()
    {
        Assert.Null(_inspector.Inspect("text/plain; charset=utf-8", "notes.txt", 10));
    }

    [Fact]
    public void Inspect_AcceptsBothSpellingsOfTheJpegExtension()
    {
        Assert.Null(_inspector.Inspect("image/jpeg", "scan.jpg", 10));
        Assert.Null(_inspector.Inspect("image/jpeg", "scan.jpeg", 10));
    }

    [Fact]
    public void Inspect_AcceptsANameWithoutAnExtension()
    {
        // Some clients send none; that is not a contradiction, only an absence.
        Assert.Null(_inspector.Inspect("application/pdf", "umowa", 10));
    }

    [Fact]
    public void Inspect_RefusesAnEmptyFile()
    {
        Assert.Equal(
            UploadInspector.EmptyFileMessage,
            _inspector.Inspect("application/pdf", "umowa.pdf", 0)
        );
    }

    [Fact]
    public void Inspect_RefusesAFileOverTheLimit()
    {
        Assert.Equal(
            UploadInspector.TooLargeMessage,
            _inspector.Inspect("application/pdf", "umowa.pdf", MaxSize + 1)
        );
    }

    [Theory]
    [InlineData("application/x-msdownload")]
    [InlineData("application/octet-stream")]
    [InlineData("")]
    public void Inspect_RefusesATypeOutsideTheAllowList(string contentType)
    {
        Assert.Equal(
            UploadInspector.UnsupportedTypeMessage,
            _inspector.Inspect(contentType, "payload.bin", 10)
        );
    }

    [Fact]
    public void Inspect_RefusesAnExtensionThatContradictsTheType()
    {
        Assert.Equal(
            UploadInspector.ExtensionMismatchMessage,
            _inspector.Inspect("application/pdf", "umowa.exe", 10)
        );
    }

    [Fact]
    public void ExtensionFor_ReturnsTheExtensionTheTypeIsStoredUnder()
    {
        Assert.Equal(".pdf", _inspector.ExtensionFor("application/pdf"));
    }

    [Fact]
    public void Config_ShipsWithAnAllowListSoAMissingSectionIsNotAnOpenDoor()
    {
        Assert.NotEmpty(new FileServiceConfig().AllowedTypes);
    }
}
