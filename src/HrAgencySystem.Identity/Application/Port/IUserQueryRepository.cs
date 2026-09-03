using HrAgencySystem.Identity.Domain;
using HrAgencySystem.Identity.Projections;
using HrAgencySystem.SharedKernel.Web;

namespace HrAgencySystem.Identity.Application.Port;

public interface IUserQueryRepository
{
    Task<SliceResponse<UserProjection>> GetUsers(Guid organizationId, string search, IReadOnlyList<OrganizationRole> roles, int page, int pageSize, CancellationToken ct);
}