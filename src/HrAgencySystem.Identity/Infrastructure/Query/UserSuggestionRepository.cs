using HrAgencySystem.Identity.Application.Model;
using HrAgencySystem.Identity.Application.Port;
using HrAgencySystem.Identity.Domain;
using HrAgencySystem.Identity.Projections;
using Marten;

namespace HrAgencySystem.Identity.Infrastructure.Query;

public sealed class UserSuggestionRepository(IDocumentSession session) : IUserSuggestionRepository
{
    public async Task<IReadOnlyList<UserSuggestion>> GetUserSuggestions(Guid organizationId, string search,
        IReadOnlyList<OrganizationRole> roles, CancellationToken ct)
    {
        return await session.Query<UserProjection>()
            .WithOrganizationId(organizationId)
            .WithSearch(search)
            .WithRoles(roles)
            .OrderByDescending(z => z.CreatedBy)
            .Take(25)
            .Select(z => new UserSuggestion(z.Id, z.FullName, z.Email))
            .ToListAsync(ct);
    }
}