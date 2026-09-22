namespace HrAgencySystem.Agency.Domain.TimeSheets;

/// <summary>
/// How long somebody worked on one day, entered as hours plus minutes.
/// <para>
/// Minutes come in steps of five, which sounds like a detail and is the single most useful rule
/// carried over from the old register: it removes a whole class of typo and makes a month's total
/// come out round. The range stops at 55 because anything above it is an hour.
/// </para>
/// </summary>
public sealed record WorkDuration
{
    public const int MinuteStep = 5;
    public const int MaxTotalMinutes = 24 * 60;

    public const string HoursRangeMessage = "Hours must be between 0 and 24.";
    public const string MinutesRangeMessage = "Minutes must be between 0 and 55.";
    public const string MinuteStepMessage = "Minutes are recorded in steps of five.";
    public const string EmptyMessage =
        "A day with no time on it is not an entry. Remove the day instead.";
    public const string TooLongMessage = "A single day cannot hold more than 24 hours.";

    private WorkDuration(int totalMinutes) => TotalMinutes = totalMinutes;

    public int TotalMinutes { get; }

    public int Hours => TotalMinutes / 60;

    public int Minutes => TotalMinutes % 60;

    public static (WorkDuration? value, string? error) TryCreate(int hours, int minutes)
    {
        if (hours is < 0 or > 24)
            return (null, HoursRangeMessage);

        if (minutes is < 0 or > 55)
            return (null, MinutesRangeMessage);

        if (minutes % MinuteStep != 0)
            return (null, MinuteStepMessage);

        var total = (hours * 60) + minutes;

        if (total == 0)
            return (null, EmptyMessage);

        if (total > MaxTotalMinutes)
            return (null, TooLongMessage);

        return (new WorkDuration(total), null);
    }
}
