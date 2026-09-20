using HrAgencySystem.Workers.Application.Port;
using Marten;

namespace HrAgencySystem.Workers.Infrastructure.Persistence;

public sealed class WorkerEmailReservationRepository(IDocumentSession session)
    : IWorkerEmailReservationRepository
{
    public async Task<bool> ExistsAsync(Guid organizationId, string email, CancellationToken ct) =>
        await session
            .Query<WorkerEmailReservation>()
            .Where(r => r.OrganizationId == organizationId && r.Email == email)
            .AnyAsync(ct);

    public Task ReserveAsync(Guid organizationId, Guid workerId, string email)
    {
        session.Insert(new WorkerEmailReservation(Guid.NewGuid(), organizationId, workerId, email));

        return Task.CompletedTask;
    }

    public async Task ChangeEmailAsync(
        Guid organizationId,
        Guid workerId,
        string? email,
        CancellationToken ct
    )
    {
        var reservation = await session
            .Query<WorkerEmailReservation>()
            .Where(r => r.OrganizationId == organizationId && r.WorkerId == workerId)
            .SingleOrDefaultAsync(ct);

        // An address can be added to a file that never had one, corrected, or taken away again.
        if (email is null)
        {
            if (reservation is not null)
                session.Delete(reservation);

            return;
        }

        if (reservation is null)
        {
            await ReserveAsync(organizationId, workerId, email);

            return;
        }

        if (reservation.Email == email)
            return;

        session.Update(reservation with { Email = email });
    }
}
