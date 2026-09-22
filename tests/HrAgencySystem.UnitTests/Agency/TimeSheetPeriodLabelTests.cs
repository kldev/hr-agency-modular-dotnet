using HrAgencySystem.Agency.Application.TimeSheets;

namespace HrAgencySystem.UnitTests.Agency;

/// <summary>
/// The month as the mail says it. Worth a test of its own because it is the one piece of the three
/// notifications that is built rather than copied, and a wrong month name in a mail about hours is
/// the kind of mistake that makes somebody open the wrong sheet.
/// </summary>
public class TimeSheetPeriodLabelTests
{
    [Theory]
    [InlineData(1, "January 2026")]
    [InlineData(2, "February 2026")]
    [InlineData(9, "September 2026")]
    [InlineData(12, "December 2026")]
    public void For_NamesTheMonth(int month, string expected)
    {
        Assert.Equal(expected, TimeSheetPeriodLabel.For(2026, month));
    }

    /// <summary>December and the January after it are different months, not the same one twice.</summary>
    [Fact]
    public void For_CarriesTheYearAcrossTheTurnOfIt()
    {
        Assert.Equal("December 2026", TimeSheetPeriodLabel.For(2026, 12));
        Assert.Equal("January 2027", TimeSheetPeriodLabel.For(2027, 1));
    }
}
