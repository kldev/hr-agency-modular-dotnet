using System.Globalization;

namespace HrAgencySystem.Agency.Application.TimeSheets;

/// <summary>
/// "September 2026" - the month as a person reads it, for the mail that announces a decision about
/// it. Written here rather than in each template because liquid has no way to name a month from a
/// year and a number, and three copies of a table of twelve names is three chances to be wrong.
/// <para>
/// Invariant culture, like the rest of the mail: every template in this system is English.
/// </para>
/// </summary>
public static class TimeSheetPeriodLabel
{
    public static string For(int year, int month) =>
        $"{CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(month)} {year.ToString(CultureInfo.InvariantCulture)}";
}
