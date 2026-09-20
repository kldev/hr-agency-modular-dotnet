namespace HrAgencySystem.FileService.UnitTests;

using FileName = HrAgencySystem.FileService.Domain.FileName;

public sealed class FileNameTests
{
    [Theory]
    [InlineData("../../etc/passwd", "passwd")]
    [InlineData("..\\..\\windows\\system32\\config", "config")]
    [InlineData("/absolute/path/report.pdf", "report.pdf")]
    [InlineData("C:\\Users\\someone\\umowa.pdf", "umowa.pdf")]
    public void Sanitize_KeepsOnlyTheLastSegment(string input, string expected)
    {
        Assert.Equal(expected, FileName.Sanitize(input));
    }

    [Fact]
    public void Sanitize_DropsControlCharacters()
    {
        Assert.Equal("report.pdf", FileName.Sanitize("re\u0000port\u000b.pdf"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("..")]
    [InlineData("/")]
    public void Sanitize_FallsBackWhenNothingUsableIsLeft(string? input)
    {
        Assert.Equal(FileName.Fallback, FileName.Sanitize(input));
    }

    [Fact]
    public void Sanitize_TruncatesALongNameButKeepsItsExtension()
    {
        var result = FileName.Sanitize(new string('a', 400) + ".pdf");

        Assert.Equal(FileName.MaxLength, result.Length);
        Assert.EndsWith(".pdf", result, StringComparison.Ordinal);
    }

    [Fact]
    public void Sanitize_TruncatesALongNameWithoutAnExtension()
    {
        var result = FileName.Sanitize(new string('a', 400));

        Assert.Equal(FileName.MaxLength, result.Length);
    }
}
