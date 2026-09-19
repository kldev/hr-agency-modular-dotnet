using HrAgencySystem.SharedKernel.Tenant;

namespace HrAgencySystem.Teams.Application.Suggestion;

public interface ITeamSuggestionRepository
{
    Task<IReadOnlyList<TeamSuggestion>> GetTeamSuggestions(
        OrganizationId organizationId,
        string search,
        CancellationToken ct
    );

    Task<TeamSuggestion?> GetTeamSuggestion(
        OrganizationId organizationId,
        Guid teamId,
        CancellationToken ct
    );
}
