using HrAgencySystem.Identity.Application.Port;
using HrAgencySystem.SharedKernel.ValueObjects;
using Marten;

namespace HrAgencySystem.Identity.Infrastructure.Persistence;

public sealed class OwnerEmailReservationRepository(IDocumentSession session) : IOwnerEmailReservationRepository
{
    public Task<bool> ExistAsync(Email email, CancellationToken ct)
    {
        return session.Query<OwnerEmailReservation>().Where(z => z.Email == email.Value).AnyAsync(ct);
    }

    public Task ReserveAsync(Email email, string passwordHash)
    {
        var insert = new OwnerEmailReservation(Guid.NewGuid(), email.Value, passwordHash);
        
        session.Insert(insert);

        return Task.CompletedTask;
    }
}