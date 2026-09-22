using HrAgencySystem.Agency.Domain.TimeSheets;

namespace HrAgencySystem.UnitTests.Agency;

/// <summary>
/// The rules that decide what counts as a day. All of them came from a register that had been in
/// use for years, and every one of them earned its place there.
/// </summary>
public class WorkDayRulesTests : BaseTest
{
    [Theory]
    [InlineData(8, 0)]
    [InlineData(7, 30)]
    [InlineData(0, 5)]
    [InlineData(24, 0)]
    public void Duration_AcceptsWholeFiveMinuteSteps(int hours, int minutes)
    {
        var (duration, error) = WorkDuration.TryCreate(hours, minutes);

        Assert.Null(error);
        Assert.Equal((hours * 60) + minutes, duration!.TotalMinutes);
    }

    /// <summary>
    /// The detail that looks like fussiness and is not: it removes a whole class of typo and makes
    /// the month's total come out round.
    /// </summary>
    [Theory]
    [InlineData(1)]
    [InlineData(7)]
    [InlineData(23)]
    public void Duration_RefusesMinutesOffTheFiveMinuteStep(int minutes)
    {
        var (_, error) = WorkDuration.TryCreate(8, minutes);

        Assert.Equal(WorkDuration.MinuteStepMessage, error);
    }

    [Fact]
    public void Duration_RefusesSixtyMinutes_BecauseThatIsAnHour()
    {
        var (_, error) = WorkDuration.TryCreate(7, 60);

        Assert.Equal(WorkDuration.MinutesRangeMessage, error);
    }

    [Fact]
    public void Duration_RefusesADayWithNothingOnIt()
    {
        var (_, error) = WorkDuration.TryCreate(0, 0);

        Assert.Equal(WorkDuration.EmptyMessage, error);
    }

    [Fact]
    public void Duration_RefusesMoreThanADay()
    {
        var (_, error) = WorkDuration.TryCreate(24, 5);

        Assert.Equal(WorkDuration.TooLongMessage, error);
    }

    /// <summary>
    /// A night shift is a normal day. It ends on the next date and still belongs to the one it
    /// started on, which is what keeps "how many days did they work" answerable.
    /// </summary>
    [Fact]
    public void ADayCanRunPastMidnight_AndStillBelongsToTheDayItStartedOn()
    {
        var day = new WorkDay(new DateOnly(2026, 9, 10), new TimeOnly(22, 0), 8 * 60, "");

        Assert.True(day.CrossesMidnight);
        Assert.Equal(new DateOnly(2026, 9, 11), DateOnly.FromDateTime(day.EndsAt));
        Assert.Equal(new DateOnly(2026, 9, 10), day.Date);
    }

    [Fact]
    public void Period_RefusesAMonthThatHasNotStarted()
    {
        var period = TimeSheetPeriod.Create(2026, 10);

        Assert.False(period.HasStartedBy(new DateOnly(2026, 9, 30)));
    }

    /// <summary>People fill their hours in as they go, so the month they are in has to be open.</summary>
    [Fact]
    public void Period_TheCurrentMonthCounts()
    {
        var period = TimeSheetPeriod.Create(2026, 9);

        Assert.True(period.HasStartedBy(new DateOnly(2026, 9, 1)));
        Assert.True(period.HasStartedBy(new DateOnly(2026, 9, 21)));
    }

    [Fact]
    public void Period_KnowsWhichDaysAreItsOwn()
    {
        var period = TimeSheetPeriod.Create(2026, 9);

        Assert.True(period.Contains(new DateOnly(2026, 9, 30)));
        Assert.False(period.Contains(new DateOnly(2026, 10, 1)));
        Assert.False(period.Contains(new DateOnly(2026, 8, 31)));
    }

    /// <summary>Saving the same date twice is a correction, not a second shift.</summary>
    [Fact]
    public void SavingTheSameDayTwice_ReplacesIt()
    {
        var sheet = TimeSheetScenario.Empty();

        sheet.Apply(TimeSheetScenario.DaySaved(TimeSheetScenario.Day(1), 8 * 60));
        sheet.Apply(TimeSheetScenario.DaySaved(TimeSheetScenario.Day(1), 6 * 60));

        Assert.Equal(1, sheet.FilledDays);
        Assert.Equal(6 * 60, sheet.TotalMinutes);
    }
}
