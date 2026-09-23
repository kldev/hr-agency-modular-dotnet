using System.Globalization;

namespace HrAgencySystem.ReportsService.Contracts;

/// <summary>
/// A run of whole calendar months, both ends included - "2026-04" to "2026-09". Months are counted
/// in UTC: a report is a monthly overview, and an event a few hours either side of midnight on the
/// last day moves one row by one month at most.
/// </summary>
public sealed record ReportPeriod
{
    public const string Format = "yyyy-MM";

    public const int MaxMonths = 24;

    /// <summary>Six months including the current one - long enough for a trend, short enough to read.</summary>
    public const int DefaultMonths = 6;

    public const string InvalidFromMessage = "'from' must be a month written as yyyy-MM.";
    public const string InvalidToMessage = "'to' must be a month written as yyyy-MM.";
    public const string ReversedMessage = "'from' cannot be later than 'to'.";

    public static readonly string TooLongMessage = $"A report covers at most {MaxMonths} months.";

    private ReportPeriod(DateOnly from, DateOnly to)
    {
        From = from;
        To = to;
    }

    /// <summary>The first day of the first month.</summary>
    public DateOnly From { get; }

    /// <summary>The first day of the last month.</summary>
    public DateOnly To { get; }

    /// <summary>Inclusive lower bound for timestamps.</summary>
    public DateTimeOffset StartsAt => new(From.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);

    /// <summary>Exclusive upper bound: the first moment of the month after the last one.</summary>
    public DateTimeOffset EndsBefore =>
        new(To.AddMonths(1).ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);

    public int MonthCount => (To.Year - From.Year) * 12 + To.Month - From.Month + 1;

    /// <summary>Every month of the period in order, so a month with no activity is still a row.</summary>
    public IEnumerable<DateOnly> Months() =>
        Enumerable.Range(0, MonthCount).Select(offset => From.AddMonths(offset));

    public string FromText => Write(From);

    public string ToText => Write(To);

    public static string Write(DateOnly month) =>
        month.ToString(Format, CultureInfo.InvariantCulture);

    /// <summary>
    /// Reads the two query values. A missing <paramref name="to"/> is the current month; a missing
    /// <paramref name="from"/> reaches back to make <see cref="DefaultMonths"/>.
    /// </summary>
    public static (ReportPeriod? period, string? error) TryCreate(
        string? from,
        string? to,
        DateOnly today
    )
    {
        var current = new DateOnly(today.Year, today.Month, 1);

        DateOnly last;
        if (string.IsNullOrWhiteSpace(to))
            last = current;
        else if (!TryParse(to, out last))
            return (null, InvalidToMessage);

        DateOnly first;
        if (string.IsNullOrWhiteSpace(from))
            first = last.AddMonths(-(DefaultMonths - 1));
        else if (!TryParse(from, out first))
            return (null, InvalidFromMessage);

        if (first > last)
            return (null, ReversedMessage);

        var period = new ReportPeriod(first, last);

        return period.MonthCount > MaxMonths ? (null, TooLongMessage) : (period, null);
    }

    public static ReportPeriod Create(string? from, string? to, DateOnly today)
    {
        var (period, error) = TryCreate(from, to, today);

        return period ?? throw new ArgumentException(error);
    }

    private static bool TryParse(string value, out DateOnly month) =>
        DateOnly.TryParseExact(
            value.Trim(),
            Format,
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out month
        );
}
