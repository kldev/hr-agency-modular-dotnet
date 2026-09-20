using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.Workers.Application.Port;
using Marten;

namespace HrAgencySystem.Workers.Infrastructure.Persistence;

public sealed class WorkerIdentityDocumentReservationRepository(IDocumentSession session)
    : IWorkerIdentityDocumentReservationRepository
{
    public async Task<bool> ExistsAsync(
        Guid organizationId,
        string issuingCountry,
        string number,
        CancellationToken ct
    ) =>
        await session
            .Query<WorkerIdentityDocumentReservation>()
            .Where(r =>
                r.OrganizationId == organizationId
                && r.IssuingCountry == issuingCountry
                && r.Number == number
            )
            .AnyAsync(ct);

    public Task ReserveAsync(
        Guid organizationId,
        Guid workerId,
        string issuingCountry,
        string number
    )
    {
        session.Insert(
            new WorkerIdentityDocumentReservation(
                Guid.NewGuid(),
                organizationId,
                workerId,
                issuingCountry,
                number
            )
        );

        return Task.CompletedTask;
    }

    public async Task ChangeDocumentAsync(
        Guid organizationId,
        Guid workerId,
        string issuingCountry,
        string number,
        CancellationToken ct
    )
    {
        var reservation =
            await session
                .Query<WorkerIdentityDocumentReservation>()
                .Where(r => r.OrganizationId == organizationId && r.WorkerId == workerId)
                .SingleOrDefaultAsync(ct)
            ?? throw new NotFoundException("Worker identity document reservation", workerId);

        if (reservation.IssuingCountry == issuingCountry && reservation.Number == number)
            return;

        session.Update(reservation with { IssuingCountry = issuingCountry, Number = number });
    }
}
