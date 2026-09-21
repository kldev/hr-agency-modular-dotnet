using HrAgencySystem.SharedKernel.Tenant;

namespace HrAgencySystem.Workers.Application.Suggestion;

public interface IWorkerSuggestionRepository
{
    Task<IReadOnlyList<WorkerSuggestion>> Search(
        OrganizationId organizationId,
        string search,
        int limit,
        CancellationToken ct
    );

    /// <summary>
    /// One suggestion by id, for a picker mounted with a value but no label - an edit form seeded
    /// from a record, or a wizard step the user navigated back to. Reading the whole projection
    /// would pull every document, permit and assignment of the person to render one name.
    /// </summary>
    Task<WorkerSuggestion?> ById(
        OrganizationId organizationId,
        Guid workerId,
        CancellationToken ct
    );
}
