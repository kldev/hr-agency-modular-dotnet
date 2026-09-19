using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.Teams.Application.Suggestion;
using HrAgencySystem.Teams.Projections;
using Marten;

namespace HrAgencySystem.Teams.Infrastructure.Query;

public sealed class TeamSuggestionRepository(IDocumentSession session) : ITeamSuggestionRepository
{
    public async Task<IReadOnlyList<TeamSuggestion>> GetTeamSuggestions(
        OrganizationId organizationId,
        string search,
        CancellationToken ct
    )
    {
        var result = await session
            .Query<TeamProjection>()
            .WithOrganizationId(organizationId)
            .WithSearch(search)
            .OrderBy(t => t.Name)
            .Take(25)
            .ToListAsync(ct);

        return [.. result.Select(t => t.ToSuggestion())];
    }

    public async Task<TeamSuggestion?> GetTeamSuggestion(
        OrganizationId organizationId,
        Guid teamId,
        CancellationToken ct
    )
    {
        var result = await session
            .Query<TeamProjection>()
            .WithOrganizationId(organizationId)
            .WithTeamId(teamId)
            .FirstOrDefaultAsync(ct);

        return result?.ToSuggestion();
    }
}
