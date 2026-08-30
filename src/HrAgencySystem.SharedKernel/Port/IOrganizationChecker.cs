namespace HrAgencySystem.SharedKernel.Port;

public interface IOrganizationChecker
{
    Task<bool> Exists(Guid organizationId, CancellationToken ct);
}