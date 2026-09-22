using HrAgencySystem.Agency.Application.Port;
using HrAgencySystem.Agency.Application.TimeSheets;
using HrAgencySystem.Agency.Domain.TimeSheets;
using HrAgencySystem.Agency.Projections;
using HrAgencySystem.SharedKernel.Tenant;
using Marten;

namespace HrAgencySystem.Agency.Infrastructure.Query;

// Public for the same reason as every other repository a Wolverine handler takes.
public sealed class TimeSheetQueryRepository(
    IDocumentSession session,
    IOrgStructureQueryRepository chart,
    IAgencyEmploymentQueryRepository employments
) : ITimeSheetQueryRepository
{
    public async Task<TimeSheetProjection?> GetAsync(
        OrganizationId organizationId,
        Guid userId,
        int year,
        int month,
        CancellationToken ct
    ) =>
        await session
            .Query<TimeSheetProjection>()
            .SingleOrDefaultAsync(
                sheet =>
                    sheet.OrganizationId == organizationId.Value
                    && sheet.UserId == userId
                    && sheet.Year == year
                    && sheet.Month == month,
                ct
            );

    public async Task<IReadOnlyList<TeamTimeSheetRow>> GetTeamAsync(
        OrganizationId organizationId,
        Guid supervisorUserId,
        int year,
        int month,
        CancellationToken ct
    )
    {
        // Three reads, joined here: who is below me, which of them owe hours, and what they have
        // filled in. Nothing in the store can answer that in one go - the first lives in the chart,
        // the second in the employment register and the third in the sheets.
        var subordinates = await chart.GetSubordinatesAsync(
            organizationId,
            supervisorUserId,
            wholeSubtree: true,
            ct
        );

        if (subordinates.Count == 0)
            return [];

        var covered = await employments.GetCoveredAsync(organizationId, year, month, ct);

        var people = covered.Where(employment => subordinates.Contains(employment.UserId)).ToList();

        if (people.Count == 0)
            return [];

        var userIds = people.Select(employment => employment.UserId).ToArray();

        var sheets = await session
            .Query<TimeSheetProjection>()
            .Where(sheet =>
                sheet.OrganizationId == organizationId.Value
                && sheet.Year == year
                && sheet.Month == month
                && userIds.Contains(sheet.UserId)
            )
            .ToListAsync(ct);

        return TeamMonitoring.Combine(people, sheets);
    }

    public async Task<IReadOnlyList<TimeSheetProjection>> GetForSettlementAsync(
        OrganizationId organizationId,
        int year,
        int month,
        CancellationToken ct
    ) =>
        await session
            .Query<TimeSheetProjection>()
            .Where(sheet =>
                sheet.OrganizationId == organizationId.Value
                && sheet.Year == year
                && sheet.Month == month
                && (
                    sheet.Status == TimeSheetStatus.Approved
                    || sheet.Status == TimeSheetStatus.Settled
                )
            )
            .OrderBy(sheet => sheet.User.LastName)
            .ToListAsync(ct);
}
