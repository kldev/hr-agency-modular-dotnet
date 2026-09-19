using HrAgencySystem.Identity.Application.Port;
using HrAgencySystem.Identity.Domain.ValueObjects;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.ValueObjects;
using Marten;

namespace HrAgencySystem.Identity.Infrastructure.Persistence;

public sealed class UserEmailReservationRepository(IDocumentSession session)
    : IUserEmailReservationRepository
{
    public async Task<bool> ExistAsync(
        OrganizationId organizationId,
        Email email,
        CancellationToken ct
    )
    {
        return await session
            .Query<UserEmailReservation>()
            .WithEmail(organizationId, email)
            .AnyAsync(ct);
    }

    public Task ReserveAsync(
        OrganizationId organizationId,
        Email email,
        UserId userId,
        string passwordHash
    )
    {
        var reservation = new UserEmailReservation(
            Guid.NewGuid(),
            userId.Value,
            organizationId.Value,
            email.Value,
            passwordHash
        );
        session.Insert(reservation);

        return Task.CompletedTask;
    }

    public async Task ChangeEmailAsync(
        OrganizationId organizationId,
        UserId userId,
        Email email,
        CancellationToken ct
    )
    {
        var reservation = await FindReservation(organizationId, userId, ct);

        if (reservation.Email == email.Value)
            return;

        session.Update(reservation with { Email = email.Value });
    }

    public async Task ChangePasswordAsync(
        OrganizationId organizationId,
        UserId userId,
        string passwordHash,
        CancellationToken ct
    )
    {
        var reservation = await FindReservation(organizationId, userId, ct);

        session.Update(reservation with { PasswordHash = passwordHash });
    }

    private async Task<UserEmailReservation> FindReservation(
        OrganizationId organizationId,
        UserId userId,
        CancellationToken ct
    )
    {
        var reservation = await session
            .Query<UserEmailReservation>()
            .Where(z => z.OrganizationId == organizationId.Value && z.UserId == userId.Value)
            .SingleOrDefaultAsync(ct);

        return reservation ?? throw new NotFoundException("User email reservation", userId.Value);
    }
}
