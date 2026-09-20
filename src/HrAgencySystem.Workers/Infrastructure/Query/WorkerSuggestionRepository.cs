using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.Workers.Application.Suggestion;
using HrAgencySystem.Workers.Projections;
using Marten;

namespace HrAgencySystem.Workers.Infrastructure.Query;

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class WorkerSuggestionRepository(IQuerySession session)
    : IWorkerSuggestionRepository
{
    public async Task<IReadOnlyList<WorkerSuggestion>> Search(
        OrganizationId organizationId,
        string search,
        int limit,
        CancellationToken ct
    )
    {
        var workers = await session
            .Query<WorkerProjection>()
            .WithOrganizationId(organizationId)
            .WithSearch(search)
            .OrderBy(w => w.LastName)
            .ThenBy(w => w.FirstName)
            .Take(Math.Clamp(limit, 1, 50))
            .ToListAsync(ct);

        return [.. workers.Select(w => w.ToSuggestion())];
    }
}
