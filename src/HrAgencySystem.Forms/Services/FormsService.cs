using HrAgencySystem.Forms.Domain;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Port;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;

namespace HrAgencySystem.Forms.Services;

public sealed class FormsService(
    IUserSnapshotRepository users,
    IWorkerSnapshotRepository workers,
    IOrganizationChecker checker
) : IFormsService
{
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

    public void ValidateAggregateUpdate(
        IOrganizationDomain? aggregate,
        Guid commandOrganizationId,
        string name,
        Guid id
    )
    {
        if (aggregate is null || aggregate.OrganizationId.Value != commandOrganizationId)
            throw new NotFoundException(name, id);
    }

    public async Task<SubjectSnapshot> GetSubjectAsync(
        OrganizationId organizationId,
        SubjectRef subject,
        CancellationToken ct
    )
    {
        switch (subject.Kind)
        {
            case SubjectKinds.Worker:
                var worker =
                    await workers.GetWorkerAsync(subject.Id, organizationId, ct)
                    ?? throw new NotFoundException("Worker", subject.Id);

                return new SubjectSnapshot(subject, worker.FullName, worker);

            default:
                throw new BusinessRuleException(SubjectKinds.UnknownKindMessage);
        }
    }
}
