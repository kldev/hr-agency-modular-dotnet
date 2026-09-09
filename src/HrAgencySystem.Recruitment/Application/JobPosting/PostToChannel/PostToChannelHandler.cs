using HrAgencySystem.Recruitment.Domain.JobPostings;
using HrAgencySystem.Recruitment.Events.JobPostings;
using HrAgencySystem.Recruitment.Services;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Time;
using Wolverine.Marten;

namespace HrAgencySystem.Recruitment.Application.JobPosting.PostToChannel;

// ReSharper disable once UnusedType.Global
public static class PostToChannelHandler
{
    [AggregateHandler]
    public static async Task<(JobPostedToChannel, Wolverine.Marten.Events)> Handle(
        PostToChannel command, JobPost aggregate,
       IRecruitmentService service,
        IClock clock,
        CancellationToken ct)
    {
        await service.ValidateOrganization(command.OrganizationId, ct);
        var user = await service.GetUserAsync(command.ModifiedBy, ct);

        var @event = new JobPostedToChannel(command.JobPostId, command.Channel, clock.UtcNow, user);
        var events = new List<IJobPostEvent> { @event };

        if (JobPostStatusChangePolicy.IsFinal(aggregate.Status))
            throw new BusinessRuleException(
                "Job post in final status. Change status to published before posting to channel.");

        if (aggregate.Status == JobPostStatus.Published ||
            !JobPostStatusChangePolicy.Allow(aggregate.Status, JobPostStatus.Published)) return (@event, [.. events]);

        var changeStatusChange = new JobPostStatusChanged(aggregate.Id.Value, aggregate.CompanyId.Value,
            aggregate.OrganizationId.Value, aggregate.Status, JobPostStatus.Published, clock.UtcNow, user);
        @events.Add(changeStatusChange);

        return (@event, [.. events]);
    }

}