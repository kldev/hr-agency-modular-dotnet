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
}
