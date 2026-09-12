using HrAgencySystem.Company.Domain;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Web.Common;

namespace HrAgencySystem.Company.Events;

public sealed record CompanyCreated(
    Guid CompanyId,
    Guid OrganizationId,
    string Name,
    string CountryCode,
    string TaxId,
    string RegistrationNumber,
    Industry Industry,
    string Website,
    UserSnapshot CreatedBy,
    DateTimeOffset CreatedAt,
    ContactPerson? Contact = null,
    Guid? ContactPersonId = null
);
