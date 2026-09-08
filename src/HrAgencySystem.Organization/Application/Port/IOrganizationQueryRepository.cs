using HrAgencySystem.Organization.Projections;
using HrAgencySystem.SharedKernel.Web;

namespace HrAgencySystem.Organization.Application.Port;

public interface IOrganizationQueryRepository
{
    Task<SliceResponse<OrganizationProjection>> GetSlice(int page, int pageSize, CancellationToken ct);
}