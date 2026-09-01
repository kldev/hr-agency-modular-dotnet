using HrAgencySystem.Organization.Domain.ValueObjects;
using HrAgencySystem.SharedKernel.Tenant;

namespace HrAgencySystem.Organization.Application.Port;

public interface IOrganizationSlugReservationRepository
{
    public Task<bool> Exists(OrganizationSlug slug, CancellationToken ct);

    public Task Reserve(OrganizationId organizationId, OrganizationSlug slug);

    public Task<OrganizationId?> FindBySlug(OrganizationSlug slug, CancellationToken ct);
}