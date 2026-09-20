using HrAgencySystem.SharedKernel.Tenant;

namespace HrAgencySystem.Projects.Application.Suggestion;

public interface IProjectSuggestionRepository
{
    Task<IReadOnlyList<ProjectSuggestion>> Search(
        OrganizationId organizationId,
        string search,
        int limit,
        CancellationToken ct
    );
}
