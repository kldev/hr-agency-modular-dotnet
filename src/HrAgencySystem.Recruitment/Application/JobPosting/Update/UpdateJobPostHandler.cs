using HrAgencySystem.Recruitment.Application.JobPosting.Create;
using HrAgencySystem.Recruitment.Domain.JobPostings;
using HrAgencySystem.Recruitment.Events.JobPosting;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Time;
using Wolverine.Marten;

namespace HrAgencySystem.Recruitment.Application.JobPosting.Update;

public static class UpdateJobPostHandler
{
    [AggregateHandler]
    public static async Task<(JobPostUpdated, Wolverine.Marten.Events)> Handle(
        UpdateJobPost command,
        JobPost aggregate,
        IUserSnapshotRepository snapshotRepository,
        IClock clock,
        CancellationToken ct)
    {
        var (title, summary, description,
            location, responsibilities,
            requirements, 
            skills, 
            salaryRange, 
            countryCode, 
            languageCode) = JobPostDataFactory.Create(command);

        var modifiedBy = await snapshotRepository.GetUserAsync(command.ModifiedBy, ct);
        if (modifiedBy == null)
            throw new BusinessRuleException(IUserSnapshotRepository.NotFoundMessage);
        
        if (aggregate.OrganizationId.Value != command.OrganizationId)
            throw new BusinessRuleException("Invalid organization id");
        
        var @event = new JobPostUpdated(
            command.JobPostId,
            title.Value,
            summary.Value,
            description.Value,
            [.. responsibilities.Select(z => z.Value)],
            [.. requirements.Select(x => x.Value)],
            [.. skills.Select(x => x.Value)],
            location.Value,
            countryCode.Value,
            languageCode.Value,
            command.EmploymentType,
            command.WorkMode,
            salaryRange.Currency,
            salaryRange.Min,
            salaryRange.Max,
            modifiedBy,
            clock.UtcNow);

        return (@event, [@event]);
    }
    
}