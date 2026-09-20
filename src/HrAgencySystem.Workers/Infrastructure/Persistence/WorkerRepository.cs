using HrAgencySystem.Workers.Application.Port;
using HrAgencySystem.Workers.Domain;
using Marten;

namespace HrAgencySystem.Workers.Infrastructure.Persistence;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class WorkerRepository(IDocumentSession session) : IWorkerRepository
{
    public async Task<Worker?> LoadAsync(Guid workerId, CancellationToken ct) =>
        await session.Events.AggregateStreamAsync<Worker>(workerId, token: ct);
}
