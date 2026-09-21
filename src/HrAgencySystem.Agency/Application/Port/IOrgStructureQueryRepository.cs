using HrAgencySystem.Agency.Projections;
using HrAgencySystem.SharedKernel.Tenant;

namespace HrAgencySystem.Agency.Application.Port;

/// <summary>
/// The read side of the chart. Deliberately not a SharedKernel port: the only things that will ask
/// "who approves this" are leave and timesheets, and both live in this module - a port in
/// SharedKernel with one consumer inside the same assembly is a boundary nobody crosses.
/// </summary>
public interface IOrgStructureQueryRepository
{
    Task<OrgStructureProjection?> GetStructureAsync(
        OrganizationId organizationId,
        CancellationToken ct
    );

    /// <summary>
    /// Who answers for this person, resolved rather than looked up - see
    /// <see cref="Domain.SupervisorPolicy"/>. Null when nobody is above them, which is an answer:
    /// the head of the top unit has no supervisor.
    /// </summary>
    Task<SupervisorView?> GetSupervisorAsync(
        OrganizationId organizationId,
        Guid userId,
        CancellationToken ct
    );

    Task<IReadOnlyList<Guid>> GetSubordinatesAsync(
        OrganizationId organizationId,
        Guid userId,
        bool wholeSubtree,
        CancellationToken ct
    );
}

/// <summary>
/// The answer to "who approves my leave", with enough to show it: the person, and the unit they
/// head that puts them above the asker.
/// </summary>
public sealed record SupervisorView(
    Guid UserId,
    string FullName,
    string Email,
    Guid UnitId,
    string UnitName
);
