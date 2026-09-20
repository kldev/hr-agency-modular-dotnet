using HrAgencySystem.Company.Domain;
using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Company.Events;

public sealed record CompanyProfileUpdated(
    Guid CompanyId,
    Guid OrganizationId,
    CompanyProfile Profile,
    UserSnapshot ModifiedBy,
    DateTimeOffset ModifiedAt
);
