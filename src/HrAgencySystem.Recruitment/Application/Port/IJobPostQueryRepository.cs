using HrAgencySystem.Recruitment.Domain.JobPostings;
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

public sealed record JobPostInfo(Guid Id, Guid OrganizationId, Guid CompanyId, string JobTitle, JobPostStatus Status);

public sealed record JobPostQuery(string Search, 
    Guid? CompanyId, 
    Guid? RecruiterId, 
    IReadOnlyList<JobPostStatus> Statuses, 
    IReadOnlyList<string> Languages,
int Page, int PageSize) : IPagedQuery;

public sealed record JobPostResponse(
    // ReSharper disable once NotAccessedPositionalProperty.Global
    Guid Id,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    Guid JobDescriptionId,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    Guid OrgId,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    Guid CompanyId,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    string Title,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    string Summary,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    string Description,
    string JobUrl,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    IReadOnlyList<string> Responsibilities,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    IReadOnlyList<string> Requirements,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    IReadOnlyList<string> Skills,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    string Location,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    string LanguageCode,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    string CountryCode,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    EmploymentType EmploymentType,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    WorkMode WorkMode,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    CurrencyCode CurrencyCode,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    decimal SalaryMin,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    decimal SalaryMax,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    JobPostStatus Status,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    Guid RecruiterId,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    UserSnapshot Recruiter,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    Guid CreatedById,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    UserSnapshot CreatedBy,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    Guid? ModifiedById,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    UserSnapshot? ModifiedBy,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    CompanySnapshot Company,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    IReadOnlyList<ChannelPost> Posts,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    DateTimeOffset CreatedAt,
    // ReSharper disable once NotAccessedPositionalProperty.Global
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