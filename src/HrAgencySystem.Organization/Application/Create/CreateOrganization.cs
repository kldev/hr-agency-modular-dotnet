using HrAgencySystem.SharedKernel.Commands;

namespace HrAgencySystem.Organization.Application.Create;

public sealed record CreateOrganization(
    string Name, 
    string Slug, 
    Guid CreatedBy, 
    IReadOnlyList<string> EmailDomains): ICreateCommand, IOrganizationData;

public interface IOrganizationData
{
    string Name { get; }
    string Slug { get; }
    IReadOnlyList<string> EmailDomains { get; }
}