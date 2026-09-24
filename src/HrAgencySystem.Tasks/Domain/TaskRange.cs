using HrAgencySystem.SharedKernel.Exception;

namespace HrAgencySystem.Tasks.Domain;

public enum TaskRangeKind
{
    Day,
    Week,
    Month,
}

/// <summary>
/// The period a task list covers, as a half-open <c>[From, To)</c> in UTC.
/// <para>
/// "Today" is a fact about the person looking, not about the server: at 00:30 in Warsaw it is
/// still yesterday in UTC. So the caller names its time zone and the bounds are local midnights
/// converted back - a week starts on Monday, a month on its first day. Worked out here, once, so
/// that no client has to agree with the server about where a week begins.
/// </para>
/// </summary>
public sealed record TaskRange(DateTimeOffset From, DateTimeOffset To)
{
    public const string UnknownTimeZoneMessage = "Unknown time zone.";

    public const string DefaultTimeZone = "Europe/Warsaw";

    public static TaskRange Resolve(TaskRangeKind kind, string? timeZoneId, DateTimeOffset now)
    {
        var zone = FindZone(string.IsNullOrWhiteSpace(timeZoneId) ? DefaultTimeZone : timeZoneId);
        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(now, zone).DateTime);

        var (first, next) = kind switch
        {
            TaskRangeKind.Day => (today, today.AddDays(1)),
            TaskRangeKind.Week => WeekOf(today),
            TaskRangeKind.Month => MonthOf(today),
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null),
        };

        return new TaskRange(StartOf(first, zone), StartOf(next, zone));
    }

    public bool Contains(DateTimeOffset moment) => moment >= From && moment < To;

    private static (DateOnly, DateOnly) WeekOf(DateOnly day)
    {
        // DayOfWeek counts from Sunday; a working week starts on Monday.
        var monday = day.AddDays(-(((int)day.DayOfWeek + 6) % 7));

        return (monday, monday.AddDays(7));
    }

    private static (DateOnly, DateOnly) MonthOf(DateOnly day)
    {
        var first = new DateOnly(day.Year, day.Month, 1);

        return (first, first.AddMonths(1));
    }

    /// <summary>
    /// The first moment of a local day. A few zones jump over midnight when the clocks change; the
    /// day then starts at the first minute that exists.
    /// </summary>
    private static DateTimeOffset StartOf(DateOnly day, TimeZoneInfo zone)
    {
        var local = day.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified);

        while (zone.IsInvalidTime(local))
            local = local.AddMinutes(30);

        return new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(local, zone), TimeSpan.Zero);
    }

    private static TimeZoneInfo FindZone(string id)
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(id);
        }
        catch (Exception exception)
            when (exception is TimeZoneNotFoundException or InvalidTimeZoneException)
        {
            throw new BusinessRuleException(UnknownTimeZoneMessage);
        }
    }
}
