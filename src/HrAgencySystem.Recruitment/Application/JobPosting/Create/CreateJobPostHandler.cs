using HrAgencySystem.Recruitment.Application.Service;
using HrAgencySystem.Recruitment.Domain.JobPostings;
using HrAgencySystem.Recruitment.Domain.JobPostings.ValueObjects;
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

        var organizationSlug = await GetOrganizationSlug(checker, organizationId, ct);

        var recruiter = await GetRecruiter(command, userSnapshotRepository, ct);

        var createdBy = await GetCreatedBy(command, userSnapshotRepository, ct);

        var jobDescription = await GetJobDescription(command, jobDescriptionSnapshotRepository, ct);

        var company = await GetCompany(companySnapshotRepository, jobDescription.CompanyId, ct);

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

    private static async Task<CompanySnapshot> GetCompany(ICompanySnapshotRepository companySnapshotRepository, Guid companyId, CancellationToken ct)
    {
        var company = await companySnapshotRepository.GetCompanyAsync(companyId, ct);
        return company ?? throw new BusinessRuleException(ICompanySnapshotRepository.NotFoundMessage);
    }

    private static async Task<JobDescriptionSnapshot> GetJobDescription(CreateJobPost command,
        IJobDescriptionSnapshotRepository jobDescriptionSnapshotRepository, CancellationToken ct)
    {
        var jobDescription =
            await jobDescriptionSnapshotRepository.GetAsync(command.JobDescriptionId, command.OrganizationId, ct);
        return jobDescription ?? throw new BusinessRuleException(IJobDescriptionSnapshotRepository.NotFoundMessage);
    }

    private static async Task<UserSnapshot> GetCreatedBy(CreateJobPost command, IUserSnapshotRepository userSnapshotRepository,
        CancellationToken ct)
    {
        var createdBy = await userSnapshotRepository.GetUserAsync(command.CreatedBy, ct);
        return createdBy ?? throw new BusinessRuleException(IUserSnapshotRepository.NotFoundMessage);
    }

    private static async Task<UserSnapshot> GetRecruiter(CreateJobPost command, IUserSnapshotRepository userSnapshotRepository,
        CancellationToken ct)
    {
        var recruiter = await userSnapshotRepository.GetUserAsync(command.RecruiterId, ct);
        return recruiter ?? throw new BusinessRuleException(IUserSnapshotRepository.NotFoundMessage);
    }

    private static async Task<string> GetOrganizationSlug(IOrganizationChecker checker,
        OrganizationId organizationId,  CancellationToken ct)
    {
        var organizationSlug = await checker.GetSlug(organizationId.Value, ct);

        return string.IsNullOrEmpty(organizationSlug) ? throw new BusinessRuleException(OrganizationId.OrganizationCheckMessage) : organizationSlug;
    }
}