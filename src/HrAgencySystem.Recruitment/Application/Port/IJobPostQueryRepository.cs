using HrAgencySystem.Recruitment.Domain.Posting;
using HrAgencySystem.Recruitment.Projections;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.ValueObjects;
using HrAgencySystem.SharedKernel.Web;

namespace HrAgencySystem.Recruitment.Application.Port;

public interface IJobPostQueryRepository
{
    Task<SliceResponse<JobPostResponse>> GetJobPosts(Guid organizationId, JobPostQuery query, CancellationToken ct);
    Task<JobPostInfo> GetJobPostInfo(Guid jobPostId, CancellationToken ct);
    Task<JobPostProjection?>  GetJobPost(Guid organizationId, Guid jobPostId, CancellationToken ct);
}

public sealed record JobPostInfo(Guid Id, Guid OrganizationId, Guid CompanyId, string JobTitle);

public sealed record JobPostQuery(string Search, 
    Guid? CompanyId, 
    Guid? RecruiterId, 
    IReadOnlyList<JobPostStatus> Statuses, 
    IReadOnlyList<string> Languages,
int Page, int PageSize) : IPagedQuery;

public sealed record JobPostResponse(
    Guid Id,
    Guid JobDescriptionId,
    Guid OrgId,
    Guid CompanyId,
    string Title,
    string Summary,
    string Description,
    string JobUrl,
    IReadOnlyList<string> Responsibilities,
    IReadOnlyList<string> Requirements,
    IReadOnlyList<string> Skills,
    string Location,
    string LanguageCode,
    string CountryCode,
    EmploymentType EmploymentType,
    WorkMode WorkMode,
    CurrencyCode CurrencyCode,
    decimal SalaryMin,
    decimal SalaryMax,
    JobPostStatus Status,
    Guid RecruiterId,
    UserSnapshot Recruiter,
    Guid CreatedById,
    UserSnapshot CreatedBy,
    Guid? ModifiedById,
    UserSnapshot? ModifiedBy,
    CompanySnapshot Company,
    IReadOnlyList<ChannelPost> Posts,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
)
{
    public static JobPostResponse From(
        JobPostProjection projection)
    {
        return new JobPostResponse(
            projection.Id,
            projection.JobDescriptionId,
            projection.OrgId,
            projection.CompanyId,
            projection.Title,
            projection.Summary,
            projection.Description,
            projection.PostingSlug,
            projection.Responsibilities,
            projection.Requirements,
            projection.Skills,
            projection.Location,
            projection.LanguageCode,
            projection.CountryCode,
            projection.EmploymentType,
            projection.WorkMode,
            projection.CurrencyCode,
            projection.SalaryMin,
            projection.SalaryMax,
            projection.Status,
            projection.Recruiter.Id,
            projection.Recruiter,
            projection.CreatedBy.Id,
            projection.CreatedBy,
            projection.ModifiedById,
            projection.ModifiedBy,
            projection.Company,
            projection.Posts,
            projection.CreatedAt,
            projection.CreatedAt
         );
    }
}