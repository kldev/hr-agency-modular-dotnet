using HrAgencySystem.Identity.Application.Port;
using HrAgencySystem.Identity.Domain.ValueObjects;
using HrAgencySystem.Identity.Infrastructure.Persistence;
using HrAgencySystem.Identity.Projections;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.ValueObjects;
using Marten;
using Wolverine;

namespace HrAgencySystem.Identity.Infrastructure.IAM;

public sealed class AccountRepository(IDocumentSession session, IMessageBus bus) : IAccountRepository
{
    public async Task<UserEmailReservation?> FindUserByEmail(Email email, string slug, CancellationToken ct)
    {
        var organizationId = await bus.InvokeAsync<OrganizationId>(slug, ct);
        var reservation = await session.Query<UserEmailReservation>().WithEmail(organizationId, email)
            .SingleOrDefaultAsync(ct);

        return reservation;

    }

    public async Task<OwnerEmailReservation?> FindOwnerByEmail(Email email, CancellationToken ct)
    {
        var reservation = await session.Query<OwnerEmailReservation>().Where(z => z.Email == email.Value)
            .SingleOrDefaultAsync(ct);

        return reservation;
    }

    public async Task<OwnerProjection> GetOwner(PlatformOwnerId id, CancellationToken ct)
    {
        return await session.Query<OwnerProjection>().Where(z => z.Id == id.Value).FirstAsync(ct);
    }

    public async Task<UserProjection> GetUser(UserId userId, CancellationToken ct)
    {
        return await session.Query<UserProjection>().Where(z => z.Id == userId.Value).FirstAsync(ct);
    }
}