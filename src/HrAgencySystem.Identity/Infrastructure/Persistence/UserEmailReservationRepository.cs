using HrAgencySystem.Identity.Application.Port;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.ValueObjects;
using Marten;

namespace HrAgencySystem.Identity.Infrastructure.Persistence;

public sealed class UserEmailReservationRepository(
    IDocumentSession session) : IUserEmailReservationRepository
{
    public async Task<bool> ExistAsync(OrganizationId organizationId, Email email, CancellationToken ct)
    {
        return await session.Query<UserEmailReservation>().WithEmail(organizationId, email).AnyAsync(ct);
    }

    public Task ReserveAsync(OrganizationId organizationId, Email email, string passwordHash)
    {
        var reservation = new UserEmailReservation(Guid.NewGuid(), organizationId.Value, email.Value, passwordHash);
        session.Insert(reservation);
        
        return Task.CompletedTask;
    }
}