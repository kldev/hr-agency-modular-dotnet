using HrAgencySystem.JobDescription.Domain;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.JobDescription.Application.Commands;

public sealed record CreateJobDescription(
    Guid Id, 
    Guid OrganizationId,
    Guid CompanyId,
    string Title,
    string? Summary,
    string Description,
    IReadOnlyList<string> Responsibilities,
    IReadOnlyList<string> Requirements,
    IReadOnlyList<string> Skills,
    string Location,
    string CountryCode,
    EmploymentType EmploymentType,
    WorkMode WorkMode,
    CurrencyCode  CurrencyCode,
    decimal SalaryMin,
    decimal SalaryMax,
    Guid RecruiterId
    );