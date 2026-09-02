using HrAgencySystem.JobDescription.Domain;
using HrAgencySystem.JobDescription.Domain.ValueObjects;
using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.JobDescription.Events;

public sealed record JobDescriptionUpdated(
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
    SalaryRange SalaryRange,
    UserSnapshot ModifiedBy,
    DateTimeOffset UpdatedAt);