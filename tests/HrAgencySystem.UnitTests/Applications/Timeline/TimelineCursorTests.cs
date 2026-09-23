using HrAgencySystem.Recruitment.Application.Timeline.Queries;
using HrAgencySystem.SharedKernel.Exception;

namespace HrAgencySystem.UnitTests.Applications.Timeline;

public class TimelineCursorTests
{
    [Fact]
    public void Parse_OfAnEncodedCursor_ReturnsTheSamePosition()
    {
        var cursor = new TimelineCursor(
            new DateTimeOffset(2026, 9, 23, 14, 30, 0, 123, TimeSpan.FromHours(2)),
            4711
        );

        var parsed = TimelineCursor.Parse(cursor.Encode());

        Assert.NotNull(parsed);
        Assert.Equal(cursor.OccurredAt, parsed.OccurredAt);
        Assert.Equal(cursor.Sequence, parsed.Sequence);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void Parse_WithoutACursor_MeansTheFirstPage(string? value)
    {
        Assert.Null(TimelineCursor.Parse(value));
    }

    [Theory]
    [InlineData("not-a-cursor")]
    [InlineData("%%%")]
    [InlineData("MTIzNA")] // "1234" - no sequence
    [InlineData("YWJjOjE")] // "abc:1" - no timestamp
    public void Parse_OfSomethingItDidNotIssue_IsRefused(string value)
    {
        var exception = Assert.Throws<InValidValueException>(() => TimelineCursor.Parse(value));

        Assert.Equal(TimelineCursor.InvalidCursorMessage, exception.Message);
    }
}
