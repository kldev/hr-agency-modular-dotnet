using HrAgencySystem.Company.Domain;
using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Company.Events;

public sealed record CompanyUpdated ( 
    Guid CompanyId,
    Guid OrganizationId,
    string Name,
    Industry Industry,
    string Website,
    string RegistrationNumber,
    string CountryCode,
    UserSnapshot ModifiedBy,
    DateTimeOffset ModifiedAt);