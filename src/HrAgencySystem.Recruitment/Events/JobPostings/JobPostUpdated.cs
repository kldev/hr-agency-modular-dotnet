using HrAgencySystem.Recruitment.Application.JobPosting.Create;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.Recruitment.Events.JobPostings;

public sealed record JobPostUpdated(    
    Guid JobPostId,
    string Title,
    string? Summary,
    string Description,
    IReadOnlyList<string> Responsibilities,
    IReadOnlyList<string> Requirements,
    IReadOnlyList<string> Skills,
    string Location,
    string CountryCode,
    string LanguageCode,
    EmploymentType EmploymentType,
    WorkMode WorkMode,
    CurrencyCode CurrencyCode,
    decimal SalaryMin,
    decimal SalaryMax,
    UserSnapshot Author,
    DateTimeOffset OccurredAt) : IJobPostData, IJobPostEvent;