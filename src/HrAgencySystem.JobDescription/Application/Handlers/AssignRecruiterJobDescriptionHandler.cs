using HrAgencySystem.JobDescription.Application.Commands;
using HrAgencySystem.JobDescription.Events;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Time;
using Wolverine.Marten;

namespace HrAgencySystem.JobDescription.Application.Handlers;

public static class AssignRecruiterJobDescriptionHandler
{
    [AggregateHandler]
    public static async Task<(JobDescriptionRecruiterAssigned,Wolverine.Marten.Events)> Handle(
        AssignRecruiterJobDescription command,
        Domain.JobDescription aggregate,
        IUserSnapshotRepository snapshotRepository,
        IClock clock,
        CancellationToken ct)
    {
        if (aggregate == null) throw new NotFoundException("Not found " + command.JobDescriptionId);

        var recruiter = await snapshotRepository.GetUserAsync(command.RecruiterId, ct);
        if (recruiter == null)
        {
            throw new BusinessRuleException(IUserSnapshotRepository.NotFoundMessage);
        }

        var modifiedBy = await snapshotRepository.GetUserAsync(command.ModifiedBy, ct);
        if (modifiedBy == null)
        {
            throw new BusinessRuleException(IUserSnapshotRepository.NotFoundMessage);
        }

        if (aggregate.OrganizationId.Value != command.OrganizationId)
            throw new BusinessRuleException(OrganizationId.OrganizationNotMatchMessage);

        var @event = new JobDescriptionRecruiterAssigned(recruiter, modifiedBy, clock.UtcNow);

        return (@event, [@event]);
    }
}