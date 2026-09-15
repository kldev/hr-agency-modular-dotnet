using HrAgencySystem.Recruitment.Application.JobApplications.Queries;
using HrAgencySystem.Recruitment.Application.Port;
using HrAgencySystem.Recruitment.Domain.Applications;
using HrAgencySystem.Recruitment.Events.Applications;
using HrAgencySystem.Recruitment.Projections;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Port;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.SharedKernel.ValueObjects;
using Marten;

namespace HrAgencySystem.Recruitment.Services;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class RecruitmentService(
    IUserSnapshotRepository userSnapshotRepository,
    ICompanySnapshotRepository companySnapshotRepository,
    IOrganizationChecker checker,
    IJobApplicationInfoQueryRepository applicationInfoQueryRepository,
    IDocumentSession session,
    IClock clock,
    INoteRepository noteRepository
    ) : IRecruitmentService
{
    public async Task<UserSnapshot> GetUserAsync(Guid userId, CancellationToken ct)
    {
        var user = await userSnapshotRepository.GetUserAsync(userId, ct);
        return user ?? throw new NotFoundException("User", userId);
    }

    public async Task<CompanySnapshot> GetCompanyAsync(Guid companyId, CancellationToken ct)
    {
        var company = await companySnapshotRepository.GetCompanyAsync(companyId, ct);
        return company ?? throw new NotFoundException("Company", companyId);
    }

    public async Task ValidateOrganization(Guid organizationId, CancellationToken ct)
    {
        var checkOrganization = await checker.Exists(organizationId, ct);
        if (!checkOrganization)
            throw new BusinessRuleException(IOrganizationChecker.OrganizationCheckMessage);
    }
    
    public async Task<JobApplicationInfo> GetApplicationAsync( 
        Guid jobApplicationId, Guid organizationId,
        CancellationToken ct)
    {
        var application = await applicationInfoQueryRepository.GetAsync(jobApplicationId, OrganizationId.From(organizationId), ct);
        return application ?? throw new NotFoundException("Job application", jobApplicationId);
    }

    public async Task<string> GetOrganizationSlug(OrganizationId organizationId, CancellationToken ct)
    {
        var slug = await checker.GetSlug(organizationId.Value, ct);
        return string.IsNullOrEmpty(slug)
            ? throw new BusinessRuleException(IOrganizationChecker.OrganizationCheckMessage)
            : slug;
    }
    
    public void ValidateAggregateUpdate(IOrganizationDomain aggregate, Guid commandOrganizationId)
    {
        if (aggregate == null || aggregate.OrganizationId.Value != commandOrganizationId)
            throw new OrganizationAccessDeniedException();
    }

    public async Task AppendApplicationNoteToStream(JobApplicationId jobApplicationId, OrganizationId organizationId,
        string note, UserSnapshot user, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(note)) return;

        var (shortNote, error) = ShortNote.TryCreate(note, false);
        if (error != null) throw new ValidationException(error);

        var application = await GetApplicationAsync(jobApplicationId.Value, organizationId.Value, ct);
        var noteEvent = new JobApplicationNoteAdded(jobApplicationId.Value, application.CandidateId,
            clock.UtcNow, shortNote!.Value, user);
        
        session.Events.Append(jobApplicationId.Value, @noteEvent);

        await noteRepository.CreateNoteAsync(
            new CreateNoteDocument(application.JobApplicationId,
                organizationId.Value,
                application.CandidateId, shortNote), user);

        await session.SaveChangesAsync(ct);
    }
}