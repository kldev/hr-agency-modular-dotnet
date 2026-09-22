using HrAgencySystem.Agency.Application.Port;
using HrAgencySystem.Agency.Projections;

namespace HrAgencySystem.Agency.Application.TimeSheets;

/// <summary>
/// Turns "who owes hours" and "who has written some" into the monitoring list.
/// <para>
/// Pure, and pulled out of the repository on purpose: the two database reads are plumbing, while
/// the join is the rule - everybody covered appears, whether or not a sheet exists for them. That
/// is the whole point of the screen and it deserves to be testable without a database.
/// </para>
/// </summary>
public static class TeamMonitoring
{
    public static IReadOnlyList<TeamTimeSheetRow> Combine(
        IReadOnlyList<AgencyEmploymentProjection> covered,
        IReadOnlyList<TimeSheetProjection> sheets
    )
    {
        var byUser = sheets
            .GroupBy(sheet => sheet.UserId)
            .ToDictionary(group => group.Key, group => group.First());

        return
        [
            .. covered
                .Select(employment =>
                    byUser.TryGetValue(employment.UserId, out var sheet)
                        ? new TeamTimeSheetRow(
                            sheet.UserId,
                            sheet.User,
                            sheet.Status,
                            sheet.TotalMinutes,
                            sheet.FilledDays,
                            sheet.LastEntryOn,
                            sheet.SubmittedAt
                        )
                        // Nobody has started. A row rather than an absence: this is exactly the
                        // person the screen exists to surface, and a list of only the sheets that
                        // exist would hide them.
                        : new TeamTimeSheetRow(
                            employment.UserId,
                            employment.User,
                            null,
                            0,
                            0,
                            null,
                            null
                        )
                )
                .OrderBy(row => row.User.LastName)
                .ThenBy(row => row.User.FirstName),
        ];
    }
}
