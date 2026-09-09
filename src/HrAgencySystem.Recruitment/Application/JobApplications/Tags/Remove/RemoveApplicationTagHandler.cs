using HrAgencySystem.Recruitment.Application.JobApplications.Tags.Queries;
using HrAgencySystem.Recruitment.Application.Port;
using HrAgencySystem.Recruitment.Events.Applications;
using HrAgencySystem.Recruitment.Services;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Port;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Time;
using Wolverine.Marten;

namespace HrAgencySystem.Recruitment.Application.JobApplications.Tags.Remove;

// ReSharper disable once UnusedType.Global
public static class RemoveApplicationTagHandler
{
    [AggregateHandler]
    public static async Task<(JobApplicationTagRemoved, Wolverine.Marten.Events)> Handle(
        RemoveApplicationTag command, 
        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Global
        Domain.Applications.JobApplication aggregate,
        ITagRepository tagRepository,
        IRecruitmentService service,
        IClock clock,
        CancellationToken ct)
    {
        if (aggregate.OrganizationId.Value != command.OrganizationId)
            throw new BusinessRuleException(IOrganizationChecker.OrganizationCheckMessage);
        
        var tag = await tagRepository.GetTag(command.TagId, ct);
        var user = await service.GetUserAsync(command.ModifiedBy, ct);

        var @event = new JobApplicationTagRemoved(command.JobApplicationId, tag, user, clock.UtcNow);
        
        return (@event, [@event]);
    }
}