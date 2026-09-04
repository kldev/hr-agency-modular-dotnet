using HrAgencySystem.Identity.Application.Port;
using HrAgencySystem.Identity.Domain;
using HrAgencySystem.Identity.Projections;
using HrAgencySystem.SharedKernel.Web;
using Marten;

namespace HrAgencySystem.Identity.Infrastructure.Query;

public class UserQueryRepository(IDocumentSession session) : IUserQueryRepository
{
    public async Task<SliceResponse<UserProjection>> GetUsers(Guid organizationId, string search, IReadOnlyList<OrganizationRole> roles, int page, int pageSize, CancellationToken ct)
    {
        var query = session.Query<UserProjection>()
            .WithOrganizationId(organizationId)
            .WithSearch(search)
            .WithRoles(roles);

        return await query.ToSlice(page, pageSize, ct);
    }

    public async Task<UserProjection?> GetUser(Guid organizationId, Guid userId, CancellationToken ct)
    {
        return await session.Query<UserProjection>()
            .WithOrganizationId(organizationId)
            .WithUserId(userId)
            .SingleOrDefaultAsync(ct);
    }
}