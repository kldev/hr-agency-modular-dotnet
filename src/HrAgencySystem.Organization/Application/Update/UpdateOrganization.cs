using HrAgencySystem.Organization.Application.Create;
using HrAgencySystem.SharedKernel.Commands;

namespace HrAgencySystem.Organization.Application.Update;

public sealed record UpdateOrganization(
    Guid OrganizationId,  
    string Name, 
    string Slug, 
    Guid ModifiedBy, 
    IReadOnlyList<string> EmailDomains): IUpdateCommand, IOrganizationData;