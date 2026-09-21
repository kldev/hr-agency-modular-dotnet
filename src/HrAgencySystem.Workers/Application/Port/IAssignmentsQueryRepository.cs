using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Web;
using HrAgencySystem.Workers.Domain;
using HrAgencySystem.Workers.Projections;

namespace HrAgencySystem.Workers.Application.Port;

public interface IAssignmentsQueryRepository
{
    Task<SliceResponse<AssignmentProjection>> GetAssignments(
        OrganizationId organizationId,
        AssignmentQuery query,
        CancellationToken ct
    );

    Task<AssignmentProjection?> GetAssignment(
        OrganizationId organizationId,
        Guid assignmentId,
        CancellationToken ct
    );

    /// <summary>
    /// Whether the person already has a posting that would run over any of these days. Nobody holds
    /// two positions at the same time, and this is what that rule is checked against.
    /// <para>
    /// Read from the projection, which the async daemon fills, so two assignments planned for the
    /// same person in the same second can still both get in. That is a deliberate limit rather than
    /// an oversight: overlapping date ranges are not something a unique index can express, and the
    /// alternative - a lock over a person's whole calendar - costs more than the mistake it prevents
    /// in a system where planning is done by people who can see the list.
    /// </para>
    /// </summary>
    /// <summary>
    /// Every posting ever held against a role, finished ones included. Read when the role is
    /// renamed, so the name frozen on each of them can be brought back into step. The person comes
    /// back with it because their own row carries a copy of the same name.
    /// </summary>
    Task<IReadOnlyList<AssignmentOnPosition>> GetAssignmentsOnPosition(
        OrganizationId organizationId,
        Guid positionId,
        CancellationToken ct
    );

    Task<bool> HasOverlappingAssignment(
        OrganizationId organizationId,
        Guid workerId,
        DateOnly startsOn,
        DateOnly? endsOn,
        Guid? exceptAssignmentId,
        CancellationToken ct
    );
}

/// <summary>One posting held against a role, and whose it is.</summary>
public sealed record AssignmentOnPosition(Guid AssignmentId, Guid WorkerId);

public sealed record AssignmentQuery(
    string Search,
    IReadOnlyList<AssignmentStatus>? Statuses,
    Guid? WorkerId,
    Guid? ProjectId,
    Guid? PositionId,
    string? WorkCountry,
    int Page,
    int PageSize
) : IPagedQuery;
