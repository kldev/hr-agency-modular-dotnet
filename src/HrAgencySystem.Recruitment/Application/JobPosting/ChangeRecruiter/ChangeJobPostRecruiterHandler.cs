using HrAgencySystem.Recruitment.Domain.Posting;
using HrAgencySystem.Recruitment.Events.JobPosting;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Time;
using Wolverine.Marten;

namespace HrAgencySystem.Recruitment.Application.JobPosting.ChangeRecruiter;

public static class ChangeJobPostRecruiterHandler
{
    [AggregateHandler]
    public static async Task<(JobPostRecruiterChanged, Wolverine.Marten.Events)> Handle(
        ChangeJobPostRecruiter command,
        JobPost aggregate,
        IUserSnapshotRepository snapshotRepository,
        IClock clock,
        CancellationToken ct)
    {
        var modifiedBy = await snapshotRepository.GetUserAsync(command.ModifiedBy, ct);
        if (modifiedBy == null)
            throw new BusinessRuleException(IUserSnapshotRepository.NotFoundMessage);
        
        if (command.RecruiterId == Guid.Empty)
            throw new InValidValueException("Recruiter id has invalid value");
        
        var recruiter = await snapshotRepository.GetUserAsync(command.RecruiterId, ct);
        if (recruiter == null)
            throw new BusinessRuleException(IUserSnapshotRepository.NotFoundMessage);
        
        if (aggregate.OrganizationId.Value != command.OrganizationId)
            throw new BusinessRuleException("Invalid organization id");

        var @event = new JobPostRecruiterChanged(command.JobPostId, recruiter, clock.UtcNow, modifiedBy);

        return (@event, [@event]);
    }
}