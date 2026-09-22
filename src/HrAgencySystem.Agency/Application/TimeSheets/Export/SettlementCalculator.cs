using HrAgencySystem.Agency.Projections;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.Agency.Application.TimeSheets.Export;

/// <summary>
/// All the arithmetic of the settlement file, kept apart from the file so it can be checked
/// without reading spreadsheet XML.
/// <para>
/// Rounding happens once, on a person's month, never per day: twenty daily roundings drift a few
/// groszy away from one monthly one, and that is the difference somebody will ask about. The amount
/// is worked out from minutes rather than from the rounded hours, so 1 h 40 min at 45 is 75.00 and
/// not 1.67 × 45. <see cref="MidpointRounding.AwayFromZero"/>, because banker's rounding surprises
/// everybody who checks the result on a calculator.
/// </para>
/// </summary>
public static class SettlementCalculator
{
    public static decimal DecimalHours(int minutes) =>
        Math.Round(minutes / 60m, 2, MidpointRounding.AwayFromZero);

    /// <summary>
    /// Hours times an hourly rate, and nothing else. A monthly or daily rate gets no amount:
    /// dividing a salary by some number of hours is a guess, and a guess in the file a transfer is
    /// made from is worse than an empty cell.
    /// </summary>
    public static decimal? AmountFor(int minutes, WorkRate? rate) =>
        rate is { Unit: RateUnit.Hourly }
            ? Math.Round(minutes * rate.Amount / 60m, 2, MidpointRounding.AwayFromZero)
            : null;

    /// <summary>
    /// One row per sheet, in the order payroll reads a list of people. Somebody with no employment
    /// record is kept, with no rate - leaving them out would hide the hours nobody can pay yet.
    /// </summary>
    public static IReadOnlyList<SettlementRow> Rows(
        IEnumerable<TimeSheetProjection> sheets,
        IEnumerable<AgencyEmploymentProjection> employments
    )
    {
        var byUser = employments.ToDictionary(employment => employment.UserId);

        return
        [
            .. sheets
                .Select(sheet =>
                {
                    var employment = byUser.GetValueOrDefault(sheet.UserId);
                    var minutes = sheet.TotalMinutes;

                    return new SettlementRow(
                        sheet.User,
                        employment?.ContractType,
                        sheet.Status,
                        minutes,
                        DecimalHours(minutes),
                        employment?.Rate,
                        AmountFor(minutes, employment?.Rate),
                        [.. sheet.Days.OrderBy(day => day.Date)]
                    );
                })
                .OrderBy(row => row.User.LastName, StringComparer.CurrentCulture)
                .ThenBy(row => row.User.FirstName, StringComparer.CurrentCulture),
        ];
    }

    public static SettlementTotal Total(IReadOnlyList<SettlementRow> rows)
    {
        var minutes = rows.Sum(row => row.Minutes);
        var priced = rows.Where(row => row.Amount is not null).ToList();
        var currencies = priced.Select(row => row.Rate!.Currency).Distinct().ToList();

        return currencies.Count == 1
            ? new SettlementTotal(
                minutes,
                DecimalHours(minutes),
                priced.Sum(row => row.Amount!.Value),
                currencies[0]
            )
            : new SettlementTotal(minutes, DecimalHours(minutes), null, null);
    }
}
