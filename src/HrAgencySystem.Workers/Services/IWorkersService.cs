using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.Workers.Domain;

namespace HrAgencySystem.Workers.Services;

/// <summary>
/// The module's one way out to everybody else. Handlers talk to this, never straight to the shared
/// kernel ports, so "what does a worker need to know about other modules" has a single answer.
/// </summary>
public interface IWorkersService
{
    public const string ProjectNotInOrganizationMessage =
        "The specified project does not exist in this organization.";

    public const string ProjectClosedMessage =
        "That project is finished, so nobody can be assigned to it.";

    Task<UserSnapshot> GetUserAsync(Guid userId, CancellationToken ct);

    /// <summary>
    /// The person a posting is about, replayed from their stream rather than read from the
    /// projection: somebody is very often marked employed in the same minute their first assignment
    /// is planned, and a read model a daemon behind would refuse a perfectly good crew. The tenant
    /// check is inside, so no caller can forget it.
    /// </summary>
    Task<Worker> GetWorkerAsync(OrganizationId organizationId, Guid workerId, CancellationToken ct);

    Task ValidateOrganization(Guid organizationId, CancellationToken ct);

    /// <summary>
    /// Resolves a project that must belong to the given organization and must still be running.
    /// Fails with a business rule rather than a 404, so the answer never reveals whether the id
    /// exists in somebody else's tenant.
    /// </summary>
    Task<ProjectSnapshot> GetProjectAsync(
        OrganizationId organizationId,
        Guid projectId,
        CancellationToken ct
    );

    void ValidateAggregateUpdate(IOrganizationDomain aggregate, Guid commandOrganizationId);
}
