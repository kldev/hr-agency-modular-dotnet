using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Port;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.Workers.Application.Port;
using HrAgencySystem.Workers.Domain;

namespace HrAgencySystem.Workers.Services;

public sealed class WorkersService(
    IUserSnapshotRepository users,
    IProjectSnapshotRepository projects,
    IPositionSnapshotRepository positions,
    IOrganizationChecker checker,
    IWorkerRepository workers
) : IWorkersService
{
    public async Task<Worker> GetWorkerAsync(
        OrganizationId organizationId,
        Guid workerId,
        CancellationToken ct
    )
    {
        var worker =
            await workers.LoadAsync(workerId, ct)
            ?? throw new NotFoundException("Worker", workerId);

        ValidateAggregateUpdate(worker, organizationId.Value);

        return worker;
    }

    public async Task<UserSnapshot> GetUserAsync(Guid userId, CancellationToken ct)
    {
        var user = await users.GetUserAsync(userId, ct);

        return user ?? throw new NotFoundException("User", userId);
    }

    public async Task ValidateOrganization(Guid organizationId, CancellationToken ct)
    {
        if (!await checker.Exists(organizationId, ct))
            throw new BusinessRuleException(IOrganizationChecker.OrganizationCheckMessage);
    }

    public async Task<ProjectSnapshot> GetProjectAsync(
        OrganizationId organizationId,
        Guid projectId,
        CancellationToken ct
    )
    {
        var project = await projects.GetProjectAsync(projectId, organizationId, ct);

        // A business rule, not a 404: answering "not found" would tell the caller whether that id
        // exists in somebody else's tenant.
        if (project is null)
            throw new BusinessRuleException(IWorkersService.ProjectNotInOrganizationMessage);

        if (!project.IsOpenForAssignments)
            throw new BusinessRuleException(IWorkersService.ProjectClosedMessage);

        return project;
    }

    public async Task<PositionSnapshot> GetPositionAsync(
        OrganizationId organizationId,
        Guid projectId,
        Guid positionId,
        CancellationToken ct
    )
    {
        var position = await positions.GetPositionAsync(projectId, positionId, organizationId, ct);

        // One refusal for an id that never existed, one that belongs to another delivery and one
        // from another agency. Whoever picked it made the same mistake either way, and a message
        // that told them apart would be telling them which ids exist elsewhere.
        if (position is null)
            throw new BusinessRuleException(IWorkersService.PositionNotOnProjectMessage);

        if (!position.IsOpenForAssignments)
            throw new BusinessRuleException(IWorkersService.PositionArchivedMessage);

        return position;
    }

    public void ValidateAggregateUpdate(IOrganizationDomain aggregate, Guid commandOrganizationId)
    {
        if (aggregate is null || aggregate.OrganizationId.Value != commandOrganizationId)
            throw new OrganizationAccessDeniedException();
    }
}
