using HrAgencySystem.Recruitment.Application.Service;
using HrAgencySystem.Recruitment.Domain.Posting;
using HrAgencySystem.Recruitment.Domain.Posting.ValueObjects;
using HrAgencySystem.Recruitment.Events.JobPosting;
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
        IOrganizationChecker checker,
        IUserSnapshotRepository userSnapshotRepository,
        ICompanySnapshotRepository companySnapshotRepository,
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

        var organizationSlug = await checker.GetSlug(organizationId.Value, ct);
        
        if (string.IsNullOrEmpty(organizationSlug))
            throw new BusinessRuleException(OrganizationId.OrganizationCheckMessage);

        var recruiter = await userSnapshotRepository.GetUserAsync(command.RecruiterId, ct);
        if (recruiter == null)
            throw new BusinessRuleException(IUserSnapshotRepository.NotFoundMessage);

        var createdBy = await userSnapshotRepository.GetUserAsync(command.CreatedBy, ct);
        if (createdBy == null)
            throw new BusinessRuleException(IUserSnapshotRepository.NotFoundMessage);

        var jobDescription =
            await jobDescriptionSnapshotRepository.GetAsync(command.JobDescriptionId, command.OrganizationId, ct);
        if (jobDescription == null)
            throw new BusinessRuleException(IJobDescriptionSnapshotRepository.NotFoundMessage);
        
        var company = await companySnapshotRepository.GetCompanyAsync(jobDescription.CompanyId, ct);
        if (company == null)
            throw new BusinessRuleException(ICompanySnapshotRepository.NotFoundMessage);
        
        var jobPostId = JobPostId.New();

        var jobPostSlug = new JobPostingSlugGenerator().Generate(
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
                recruiter!,
                createdBy,
                company,
                languageCode.Value,
                organizationSlug,
                jobPostSlug,
                clock.UtcNow);

        session.Events.StartStream<JobPost>(jobPostId.Value, @event);

        return @event;
    }
}