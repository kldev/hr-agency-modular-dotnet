using HrAgencySystem.SharedKernel.Web.Common;

namespace HrAgencySystem.Company.Documents;

public sealed record CompanyContact(
    Guid Id, 
    Guid OrganizationId,
    Guid CompanyId, 
    ContactPerson Contact,
    string CompanyName,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ModifiedAt = null);
    
    