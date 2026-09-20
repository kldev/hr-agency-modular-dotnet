using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Web;
using HrAgencySystem.Workers.Application.Port;
using HrAgencySystem.Workers.Domain;
using HrAgencySystem.Workers.Projections;
using Marten;

namespace HrAgencySystem.Workers.Infrastructure.Query;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class AssignmentsQueryRepository(IQuerySession session) : IAssignmentsQueryRepository
{
    public async Task<SliceResponse<AssignmentProjection>> GetAssignments(
        OrganizationId organizationId,
        AssignmentQuery query,
        CancellationToken ct
    )
    {
        var assignments = session
            .Query<AssignmentProjection>()
            .WithOrganizationId(organizationId)
            .WithSearch(query.Search)
            .WithFilters(query)
            .OrderByDescending(a => a.StartsOn)
            .ThenBy(a => a.Id);

        return await assignments.ToSlice(query, ct);
    }

    public async Task<AssignmentProjection?> GetAssignment(
        OrganizationId organizationId,
        Guid assignmentId,
        CancellationToken ct
    ) =>
        await session
            .Query<AssignmentProjection>()
            .WithOrganizationId(organizationId)
            .Where(a => a.Id == assignmentId)
            .FirstOrDefaultAsync(ct);

    public async Task<bool> HasOverlappingAssignment(
        OrganizationId organizationId,
        Guid workerId,
        DateOnly startsOn,
        DateOnly? endsOn,
        Guid? exceptAssignmentId,
        CancellationToken ct
    )
    {
        var candidates = await session
            .Query<AssignmentProjection>()
            .WithOrganizationId(organizationId)
            .Where(a => a.WorkerId == workerId)
            .Where(a => a.Status == AssignmentStatus.Planned || a.Status == AssignmentStatus.Active)
            .Select(a => new
            {
                a.Id,
                a.StartsOn,
                a.EndsOn,
            })
            .ToListAsync(ct);

        // The overlap itself is worked out here rather than in SQL: an open ended period is a null,
        // and "null means forever" is a rule the domain owns, not one to spell out twice in LINQ.
        return candidates.Any(a =>
            a.Id != exceptAssignmentId && Overlaps(a.StartsOn, a.EndsOn, startsOn, endsOn)
        );
    }

    private static bool Overlaps(
        DateOnly firstStart,
        DateOnly? firstEnd,
        DateOnly secondStart,
        DateOnly? secondEnd
    ) =>
        (firstEnd is null || firstEnd >= secondStart)
        && (secondEnd is null || secondEnd >= firstStart);
}
