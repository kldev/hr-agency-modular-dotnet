using HrAgencySystem.ReportsService.Contracts;

namespace HrAgencySystem.ReportsService.UnitTests;

public sealed class ReportPeriodTests
{
    private static readonly DateOnly Today = new(2026, 9, 23);

    [Fact]
    public void NothingGiven_IsTheLastSixMonthsIncludingThisOne()
    {
        var period = ReportPeriod.Create(null, null, Today);

        Assert.Equal("2026-04", period.FromText);
        Assert.Equal("2026-09", period.ToText);
        Assert.Equal(6, period.MonthCount);
        Assert.Equal(new DateTimeOffset(2026, 4, 1, 0, 0, 0, TimeSpan.Zero), period.StartsAt);
        Assert.Equal(new DateTimeOffset(2026, 10, 1, 0, 0, 0, TimeSpan.Zero), period.EndsBefore);
    }

    [Fact]
    public void Months_ListsEveryMonthAcrossAYearEnd()
    {
        var period = ReportPeriod.Create("2025-11", "2026-02", Today);

        Assert.Equal(
            ["2025-11", "2025-12", "2026-01", "2026-02"],
            period.Months().Select(ReportPeriod.Write)
        );
    }

    [Theory]
    [InlineData("2026-13", null, ReportPeriod.InvalidFromMessage)]
    [InlineData("26-01", null, ReportPeriod.InvalidFromMessage)]
    [InlineData(null, "september", ReportPeriod.InvalidToMessage)]
    [InlineData("2026-09", "2026-01", ReportPeriod.ReversedMessage)]
    public void AMalformedPeriod_IsRefusedWithTheReason(string? from, string? to, string message)
    {
        var (period, error) = ReportPeriod.TryCreate(from, to, Today);

        Assert.Null(period);
        Assert.Equal(message, error);
    }

    [Fact]
    public void MoreThanTwoYears_IsRefused()
    {
        var (period, error) = ReportPeriod.TryCreate("2024-01", "2026-01", Today);

        Assert.Null(period);
        Assert.Equal(ReportPeriod.TooLongMessage, error);
    }

    [Fact]
    public void ExactlyTwoYears_IsAllowed()
    {
        var (period, error) = ReportPeriod.TryCreate("2024-02", "2026-01", Today);

        Assert.Null(error);
        Assert.Equal(ReportPeriod.MaxMonths, period!.MonthCount);
    }
}
