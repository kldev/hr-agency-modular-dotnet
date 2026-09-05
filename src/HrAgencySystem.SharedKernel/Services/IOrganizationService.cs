namespace HrAgencySystem.SharedKernel.Services;

public interface IOrganizationService
{
    Task<IReadOnlyList<OrganizationInfo>> GetActiveOrganizationsAsync(CancellationToken ct);
}

// ReSharper disable once ClassNeverInstantiated.Global
public sealed record OrganizationInfo(Guid Id, string Slug);