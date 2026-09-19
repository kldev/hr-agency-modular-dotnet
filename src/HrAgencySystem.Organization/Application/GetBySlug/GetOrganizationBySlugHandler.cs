using HrAgencySystem.Organization.Application.Port;
using HrAgencySystem.Organization.Domain.ValueObjects;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Tenant;

namespace HrAgencySystem.Organization.Application.GetBySlug;

public static class GetOrganizationBySlugHandler
{
    public static async Task<OrganizationId> Handle(
        string slug,
        IOrganizationSlugReservationRepository repository,
        CancellationToken ct
    )
    {
        var organizationId = await repository.FindBySlug(OrganizationSlug.Create(slug), ct);
        return organizationId ?? throw new NotFoundException("Organization", slug);
    }
}
