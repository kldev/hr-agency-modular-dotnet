using HrAgencySystem.Identity.Domain;
using HrAgencySystem.Identity.Projections;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Web;

namespace HrAgencySystem.Identity.Application.Port;

public interface IUserQueryRepository
{
    Task<SliceResponse<UserProjection>> GetUsers(
        OrganizationId organizationId,
        string search,
        IReadOnlyList<OrganizationRole> roles,
        int page,
        int pageSize,
        CancellationToken ct
    );
    Task<UserProjection?> GetUser(OrganizationId organizationId, Guid userId, CancellationToken ct);
    Task<SliceResponse<UserProjection>> GetUsersOwner(
        OrganizationId? organizationId,
        string search,
        IReadOnlyList<OrganizationRole> roles,
        int page,
        int pageSize,
        CancellationToken ct
    );
}
