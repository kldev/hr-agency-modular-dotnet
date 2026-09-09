using HrAgencySystem.Recruitment.Domain.JobPostings;
using HrAgencySystem.Recruitment.Events.JobPostings;
using HrAgencySystem.Recruitment.Services;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Time;
using Wolverine.Marten;

namespace HrAgencySystem.Recruitment.Application.JobPosting.ChangeRecruiter;

// ReSharper disable once UnusedType.Global
public static class ChangeJobPostRecruiterHandler
{
    [AggregateHandler]
    public static async Task<(JobPostRecruiterChanged, Wolverine.Marten.Events)> Handle(
        ChangeJobPostRecruiter command,
        JobPost aggregate,
        IRecruitmentService service,
        IClock clock,
        CancellationToken ct)
    {
        var modifiedBy = await service.GetUserAsync(command.ModifiedBy, ct);
        var recruiter = await service.GetUserAsync(command.RecruiterId, ct);

        ValidateOrganization(command, aggregate);

        var @event = new JobPostRecruiterChanged(command.JobPostId, recruiter, clock.UtcNow, modifiedBy);

        return (@event, [@event]);
    }

    private static void ValidateOrganization(ChangeJobPostRecruiter command, JobPost aggregate)
    {
        if (aggregate.OrganizationId.Value != command.OrganizationId)
            throw new BusinessRuleException("Invalid organization id");
    }
}