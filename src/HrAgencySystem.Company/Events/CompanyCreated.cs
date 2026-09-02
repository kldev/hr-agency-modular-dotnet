using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Company.Events;

public sealed record CompanyCreated(
    Guid CompanyId,
    Guid OrganizationId,
    string Name,
    string CountryCode,
    string TaxId,
    string RegistrationNumber,
    UserSnapshot CreatedBy,
    DateTimeOffset CreatedAt);