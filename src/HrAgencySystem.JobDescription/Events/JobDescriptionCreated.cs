using HrAgencySystem.JobDescription.Domain;
using HrAgencySystem.JobDescription.Domain.ValueObjects;

namespace HrAgencySystem.JobDescription.Events;

public sealed record JobDescriptionCreated(
    Guid JobDescriptionId,
    Guid OrganizationId,
    Guid CompanyId,
    JobTitle Title,
    JobSummary? Summary,
    JobDescriptionText? Description,
    IReadOnlyList<EntryText> Responsibilities,
    IReadOnlyList<EntryText> Requirements,
    IReadOnlyList<EntryText> Skills,
    string? Location,
    string CountryCode,
    EmploymentType EmploymentType,
    WorkMode WorkMode,
    SalaryRange? SalaryRange,
    Guid RecruiterId,
    DateTimeOffset CreatedAt);