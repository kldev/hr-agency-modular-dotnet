using HrAgencySystem.SharedKernel.Commands;

namespace HrAgencySystem.Organization.Application.Create;

public sealed record CreateOrganization(
    string Name, 
    string Slug, 
    Guid CreatedBy, 
    IReadOnlyList<string> EmailDomains,
    OrganizationInfoData? Info = null) : ICreateCommand, IOrganizationData;

public interface IOrganizationData
{
    string Name { get; }
    string Slug { get; }
    IReadOnlyList<string> EmailDomains { get; }
    OrganizationInfoData? Info { get; }
}

public record OrganizationInfoData(
    string Phone = "",
    string Email = "",
    string Location = "",
    string Website = "")
{
    public static OrganizationInfoData NoInfo
        => new ();
}