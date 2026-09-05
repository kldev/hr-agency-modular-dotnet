using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.Recruitment.Events.JobPostings;

public sealed record JobPostCreated(
    Guid JobPostId,
    Guid JobDescriptionId,
    Guid OrganizationId,
    Guid CompanyId,
    string Title,
    string Summary,
    string Description,
    IReadOnlyList<string> Responsibilities,
    IReadOnlyList<string> Requirements,
    IReadOnlyList<string> Skills,
    string Location,
    string CountryCode,
    EmploymentType EmploymentType,
    WorkMode WorkMode,
    CurrencyCode CurrencyCode,
    decimal SalaryMin,
    decimal SalaryMax,
    UserSnapshot Recruiter,
    UserSnapshot CreatedBy,
    CompanySnapshot Company,
    string LanguageCode,
    string OrgSlug,
    string PostingSlug,
    DateTimeOffset CreatedAt);
