using HrAgencySystem.SharedKernel.Web.Common;

namespace HrAgencySystem.Company.Events;

public sealed record CompanyPrimaryContactUpdated(
    Guid CompanyId,
    Guid OrganizationId,
    ContactPerson Contact,
    Guid ContactPersonId,
    DateTimeOffset ModifiedAt);