using HrAgencySystem.Recruitment.Application.JobPosting.Create;
using HrAgencySystem.SharedKernel.Commands;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.Recruitment.Application.JobPosting.Update;

public sealed record UpdateJobPost(
    Guid JobPostingId,
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
    Guid ModifiedBy) : IJobPostData, IUpdateCommand;
