using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.Workers.Domain;
using Marten;

namespace HrAgencySystem.Workers.Infrastructure.Query;

/// <summary>
/// The worker for other modules. Replayed from the stream rather than read from the projection: a
/// form is very often opened for somebody registered a minute ago, and a read model a daemon behind
/// would answer "no such worker" for a perfectly real one.
/// </summary>
// ReSharper disable once ClassNeverInstantiated.Global
public sealed class WorkerSnapshotRepository(IDocumentSession session) : IWorkerSnapshotRepository
{
    public async Task<WorkerSnapshot?> GetWorkerAsync(
        Guid workerId,
        OrganizationId organizationId,
        CancellationToken ct
    )
    {
        var worker = await session.Events.AggregateStreamAsync<Worker>(workerId, token: ct);

        if (worker is null || worker.OrganizationId != organizationId)
            return null;

        return new WorkerSnapshot(
            worker.Id.Value,
            worker.FirstName.Value,
            worker.LastName.Value,
            worker.DateOfBirth,
            worker.Citizenship.Value,
            worker.Email?.Value,
            string.IsNullOrWhiteSpace(worker.PhoneNumber.Value) ? null : worker.PhoneNumber.Value
        );
    }
}
