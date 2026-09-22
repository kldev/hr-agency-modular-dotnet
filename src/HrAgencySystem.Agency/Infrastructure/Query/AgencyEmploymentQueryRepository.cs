using HrAgencySystem.Agency.Application.Port;
using HrAgencySystem.Agency.Domain;
using HrAgencySystem.Agency.Domain.Employment;
using HrAgencySystem.Agency.Projections;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;
using Marten;

namespace HrAgencySystem.Agency.Infrastructure.Query;

// Public, not internal: Wolverine generates handler code into another assembly and falls back to
// service location on an internal type, which throws at runtime.
public sealed class AgencyEmploymentQueryRepository(
    IDocumentSession session,
    IUserSnapshotRepository users
) : IAgencyEmploymentQueryRepository
{
    public async Task<AgencyEmploymentProjection?> GetAsync(
        OrganizationId organizationId,
        Guid userId,
        CancellationToken ct
    )
    {
        var projection = await session
            .Query<AgencyEmploymentProjection>()
            .SingleOrDefaultAsync(
                employment =>
                    employment.OrganizationId == organizationId.Value
                    && employment.UserId == userId,
                ct
            );

        if (projection is not null)
            return projection;

        /*
         * The same fallback the chart and the company profile make, and it is load bearing here:
         * somebody is hired and starts filling their hours in minutes later, while the async daemon
         * is still catching up. Without it the first entry of a new joiner is refused with "this
         * person has no employment record", which is both wrong and impossible to act on.
         *
         * Exact rather than a scan, because the stream id is derived from who and where.
         */
        var streamId = AgencyStreamId.ForEmployment(organizationId.Value, userId);

        var employment = await session.Events.AggregateStreamAsync<AgencyEmployment>(
            streamId,
            token: ct
        );

        if (employment is null || employment.OrganizationId != organizationId)
            return null;

        var user = await users.GetUserAsync(userId, organizationId, ct);

        return user is null
            ? null
            : new AgencyEmploymentProjection(
                streamId,
                organizationId.Value,
                userId,
                user,
                employment.ContractType,
                employment.StartsOn,
                employment.EndsOn,
                employment.WeeklyHours,
                employment.CreatedAt,
                null,
                employment.ModifiedAt
            );
    }

    public async Task<IReadOnlyList<AgencyEmploymentProjection>> GetAllAsync(
        OrganizationId organizationId,
        CancellationToken ct
    ) =>
        await session
            .Query<AgencyEmploymentProjection>()
            .Where(employment => employment.OrganizationId == organizationId.Value)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<AgencyEmploymentProjection>> GetCoveredAsync(
        OrganizationId organizationId,
        int year,
        int month,
        CancellationToken ct
    )
    {
        var first = new DateOnly(year, month, 1);
        var last = first.AddMonths(1).AddDays(-1);

        var candidates = await session
            .Query<AgencyEmploymentProjection>()
            .Where(employment =>
                employment.OrganizationId == organizationId.Value
                && employment.StartsOn <= last
                && (employment.EndsOn == null || employment.EndsOn >= first)
            )
            .ToListAsync(ct);

        /*
         * The contract-type filter is applied in memory rather than in the query: whether a kind of
         * contract carries the duty is `TimeRecordPolicy`, and translating it into a Linq predicate
         * would be a second copy of the law, kept in step by hand.
         */
        return [.. candidates.Where(employment => employment.RequiresTimeRecord)];
    }
}
