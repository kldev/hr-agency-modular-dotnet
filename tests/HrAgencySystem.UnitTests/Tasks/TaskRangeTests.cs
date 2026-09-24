using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.Tasks.Domain;

namespace HrAgencySystem.UnitTests.Tasks;

public class TaskRangeTests
{
    private const string Warsaw = "Europe/Warsaw";

    // Thursday, 24 September 2026, 08:00 UTC = 10:00 in Warsaw (CEST, +02:00).
    private static readonly DateTimeOffset Now = new(2026, 9, 24, 8, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Day_IsTheLocalDay()
    {
        var range = TaskRange.Resolve(TaskRangeKind.Day, Warsaw, Now);

        Assert.Equal(new DateTimeOffset(2026, 9, 23, 22, 0, 0, TimeSpan.Zero), range.From);
        Assert.Equal(new DateTimeOffset(2026, 9, 24, 22, 0, 0, TimeSpan.Zero), range.To);
    }

    [Fact]
    public void Day_JustAfterLocalMidnight_IsAlreadyTheNextDay()
    {
        // 22:30 UTC on the 24th is 00:30 on the 25th in Warsaw.
        var range = TaskRange.Resolve(TaskRangeKind.Day, Warsaw, new DateTimeOffset(2026, 9, 24, 22, 30, 0, TimeSpan.Zero));

        Assert.Equal(new DateTimeOffset(2026, 9, 24, 22, 0, 0, TimeSpan.Zero), range.From);
    }

    [Fact]
    public void Week_StartsOnMonday()
    {
        var range = TaskRange.Resolve(TaskRangeKind.Week, Warsaw, Now);

        Assert.Equal(new DateTimeOffset(2026, 9, 20, 22, 0, 0, TimeSpan.Zero), range.From);
        Assert.Equal(new DateTimeOffset(2026, 9, 27, 22, 0, 0, TimeSpan.Zero), range.To);
    }

    [Fact]
    public void Week_OnASunday_IsTheWeekThatEndsThatDay()
    {
        // Sunday, 27 September 2026, noon in Warsaw.
        var range = TaskRange.Resolve(TaskRangeKind.Week, Warsaw, new DateTimeOffset(2026, 9, 27, 10, 0, 0, TimeSpan.Zero));

        Assert.Equal(new DateTimeOffset(2026, 9, 20, 22, 0, 0, TimeSpan.Zero), range.From);
    }

    [Fact]
    public void Month_CrossingTheClockChange_EndsAtLocalMidnightInWinterTime()
    {
        // October ends on the 31st in CET (+01:00): the clocks go back on the 25th.
        var range = TaskRange.Resolve(TaskRangeKind.Month, Warsaw, new DateTimeOffset(2026, 10, 10, 10, 0, 0, TimeSpan.Zero));

        Assert.Equal(new DateTimeOffset(2026, 9, 30, 22, 0, 0, TimeSpan.Zero), range.From);
        Assert.Equal(new DateTimeOffset(2026, 10, 31, 23, 0, 0, TimeSpan.Zero), range.To);
    }

    [Fact]
    public void NoTimeZone_CountsInWarsaw()
    {
        Assert.Equal(TaskRange.Resolve(TaskRangeKind.Day, Warsaw, Now), TaskRange.Resolve(TaskRangeKind.Day, null, Now));
    }

    [Fact]
    public void UnknownTimeZone_IsRefused()
    {
        var error = Assert.Throws<BusinessRuleException>(() => TaskRange.Resolve(TaskRangeKind.Day, "Mars/Olympus", Now));

        Assert.Equal(TaskRange.UnknownTimeZoneMessage, error.Message);
    }

    [Fact]
    public void Contains_IsHalfOpen()
    {
        var range = TaskRange.Resolve(TaskRangeKind.Day, Warsaw, Now);

        Assert.True(range.Contains(range.From));
        Assert.False(range.Contains(range.To));
    }
}
