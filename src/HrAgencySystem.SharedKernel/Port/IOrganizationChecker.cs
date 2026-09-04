namespace HrAgencySystem.SharedKernel.Port;

public interface IOrganizationChecker
{
    public const string OrganizationCheckMessage = "Non existing organization.";
    Task<bool> Exists(Guid organizationId, CancellationToken ct);
    Task<string?> GetSlug(Guid organizationId, CancellationToken ct);
}