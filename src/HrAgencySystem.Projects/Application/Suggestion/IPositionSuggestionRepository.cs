using HrAgencySystem.SharedKernel.Tenant;

namespace HrAgencySystem.Projects.Application.Suggestion;

public interface IPositionSuggestionRepository
{
    /// <summary>
    /// Archived roles never appear here. A picker offering a closed role is an invitation to plan
    /// somebody onto it, which the domain then refuses - a missing option beats an error message.
    /// </summary>
    Task<IReadOnlyList<PositionSuggestion>> Search(
        OrganizationId organizationId,
        Guid? projectId,
        string search,
        int limit,
        CancellationToken ct
    );

    Task<PositionSuggestion?> ById(
        OrganizationId organizationId,
        Guid positionId,
        CancellationToken ct
    );
}
