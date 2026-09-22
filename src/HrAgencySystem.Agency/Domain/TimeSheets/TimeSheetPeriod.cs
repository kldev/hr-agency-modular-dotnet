namespace HrAgencySystem.Agency.Domain.TimeSheets;

/// <summary>
/// The month a sheet covers. A value, not an entity: the old system kept a row per period purely
/// so a foreign key had somewhere to point, and with a stream per sheet there is nothing to point
/// at any more.
/// </summary>
public sealed record TimeSheetPeriod
{
    public const string YearRangeMessage = "The year must be between 2000 and 2100.";
    public const string MonthRangeMessage = "The month must be between 1 and 12.";
    public const string FutureMessage = "A month that has not started yet cannot be filled in.";

    private TimeSheetPeriod(int year, int month)
    {
        Year = year;
        Month = month;
    }

    public int Year { get; }
    public int Month { get; }

    public DateOnly FirstDay => new(Year, Month, 1);

    public DateOnly LastDay => FirstDay.AddMonths(1).AddDays(-1);

    public static (TimeSheetPeriod? value, string? error) TryCreate(int year, int month)
    {
        if (year is < 2000 or > 2100)
            return (null, YearRangeMessage);

        if (month is < 1 or > 12)
            return (null, MonthRangeMessage);

        return (new TimeSheetPeriod(year, month), null);
    }

    public static TimeSheetPeriod Create(int year, int month)
    {
        var (value, error) = TryCreate(year, month);

        return value ?? throw new ArgumentException(error);
    }

    public bool Contains(DateOnly date) => date.Year == Year && date.Month == Month;

    /// <summary>
    /// The current month counts as started - people fill their hours in as they go, and refusing
    /// today would make the sheet unusable until the month was over.
    /// </summary>
    public bool HasStartedBy(DateOnly today) => FirstDay <= today;

    public override string ToString() => $"{Year:D4}-{Month:D2}";
}
