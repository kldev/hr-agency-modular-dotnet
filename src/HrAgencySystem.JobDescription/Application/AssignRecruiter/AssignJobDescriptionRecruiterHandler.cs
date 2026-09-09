using HrAgencySystem.JobDescription.Events;
using HrAgencySystem.JobDescription.Services;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Time;
using Wolverine.Marten;

namespace HrAgencySystem.JobDescription.Application.AssignRecruiter;

public static class AssignJobDescriptionRecruiterHandler
{
    [AggregateHandler]
    public static async Task<(JobDescriptionRecruiterAssigned,Wolverine.Marten.Events)> Handle(
        AssignJobDescriptionRecruiter command,
        Domain.JobDescription aggregate,
        IJobDescriptionService service,
        IClock clock,
        CancellationToken ct)
    {
        if (aggregate == null) throw new NotFoundException("Job description", command.JobDescriptionId);
        
        var recruiter = await service.GetUserAsync(command.RecruiterId, ct);

        var modifiedBy = await service.GetUserAsync(command.ModifiedBy, ct);

        ValidateOrganization(command, aggregate);

        var @event = new JobDescriptionRecruiterAssigned(recruiter, modifiedBy, clock.UtcNow);

        return (@event, [@event]);
    }

    private static void ValidateOrganization(AssignJobDescriptionRecruiter command, Domain.JobDescription aggregate)
    {
        if (aggregate.OrganizationId.Value != command.OrganizationId)
            throw new BusinessRuleException(OrganizationId.OrganizationNotMatchMessage);
    }
}