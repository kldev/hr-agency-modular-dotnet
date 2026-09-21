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

    /// <summary>
    /// One suggestion by id, for a picker mounted with a value but no label. See the same method on
    /// the worker suggestion port: the details projection is far too much to render one line.
    /// </summary>
    Task<ProjectSuggestion?> ById(
        OrganizationId organizationId,
        Guid projectId,
        CancellationToken ct
    );
}
