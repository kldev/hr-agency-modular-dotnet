namespace HrAgencySystem.SharedKernel.Services;

public interface IOrganizationService
{
    Task<IReadOnlyList<OrganizationInfo>> GetActiveOrganizationsAsync(CancellationToken ct);
    Task<OrganizationInfo?> GetBySlugAsync(string slug, CancellationToken ct);
    Task<OrganizationInfo?> GetByEmailDomainAsync(string emailDomain, CancellationToken ct);
}

// ReSharper disable once ClassNeverInstantiated.Global
public sealed record OrganizationInfo(Guid Id, string Slug, string Name);