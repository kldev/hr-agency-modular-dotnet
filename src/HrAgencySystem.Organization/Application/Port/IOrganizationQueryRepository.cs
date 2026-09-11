using HrAgencySystem.Organization.Projections;
using HrAgencySystem.SharedKernel.Web;

namespace HrAgencySystem.Organization.Application.Port;

public interface IOrganizationQueryRepository
{
    Task<SliceResponse<OrganizationProjection>> GetSlice(string? search, int page, int pageSize, CancellationToken ct);
}