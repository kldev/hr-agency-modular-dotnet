using HrAgencySystem.Recruitment.Domain.JobPostings;
using HrAgencySystem.Recruitment.Domain.JobPostings.ValueObjects;
using HrAgencySystem.Recruitment.Events.JobPostings;
using HrAgencySystem.Recruitment.Services;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Port;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Time;
using Marten;

namespace HrAgencySystem.Recruitment.Application.JobPosting.Create;

public static class CreateJobPostHandler
{
    public static async Task<JobPostCreated> Handle(
        CreateJobPost command,
        IDocumentSession session,
        IClock clock,
        IRecruitmentService service,
        IJobDescriptionSnapshotRepository  jobDescriptionSnapshotRepository,
        CancellationToken ct)
    {
        var organizationId = OrganizationId.From(command.OrganizationId);
        var jobDescriptionId = JobDescriptionId.From(command.JobDescriptionId);
        
        var (title, 
            summary, 
            description,
            location, 
            responsibilities,
            requirements, 
            skills, 
            salaryRange, 
            countryCode, 
            languageCode) = JobPostDataFactory.Create(command);

        var organizationSlug = await service.GetOrganizationSlug(organizationId, ct);

        var recruiter = await service.GetUserAsync(command.RecruiterId, ct);

        var createdBy = await service.GetUserAsync(command.CreatedBy, ct);

        var jobDescription = await GetJobDescription(command, jobDescriptionSnapshotRepository, ct);

        var company = await service.GetCompanyAsync(jobDescription.CompanyId, ct);

        var jobPostId = JobPostId.New();

        var jobPostSlug = JobPostingSlugGenerator.Generate(
            company.Name, title.Value, location.Value, jobPostId.Value);
        
        var @event = new JobPostCreated(
                jobPostId.Value,
                jobDescriptionId.Value,
                organizationId.Value,
                jobDescription.CompanyId,
                title.Value,
                summary.Value,
                description.Value,
                [.. responsibilities.Select(z => z.Value)],
                [.. requirements.Select(x => x.Value)],
                [.. skills.Select(x => x.Value)],
                location.Value,
                countryCode.Value,
                command.EmploymentType,
                command.WorkMode,
                salaryRange.Currency,
                salaryRange.Min,
                salaryRange.Max,
                recruiter,
                createdBy,
                company,
                languageCode.Value,
                organizationSlug,
                jobPostSlug,
                clock.UtcNow);

        session.Events.StartStream<JobPost>(jobPostId.Value, @event);

        return @event;
    }
    
    private static async Task<JobDescriptionSnapshot> GetJobDescription(CreateJobPost command,
        IJobDescriptionSnapshotRepository jobDescriptionSnapshotRepository, CancellationToken ct)
    {
        var jobDescription =
            await jobDescriptionSnapshotRepository.GetAsync(command.JobDescriptionId, command.OrganizationId, ct);
        return jobDescription ?? throw new BusinessRuleException(IJobDescriptionSnapshotRepository.NotFoundMessage);
    }
}