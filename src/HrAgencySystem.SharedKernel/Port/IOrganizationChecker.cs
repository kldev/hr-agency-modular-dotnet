namespace HrAgencySystem.SharedKernel.Port;

public interface IOrganizationChecker
{
    Task<bool> Exists(Guid organizationId, CancellationToken ct);
    Task<string?> GetSlug(Guid organizationId, CancellationToken ct);
}