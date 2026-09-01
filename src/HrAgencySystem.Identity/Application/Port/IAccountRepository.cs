using HrAgencySystem.Identity.Domain.ValueObjects;
using HrAgencySystem.Identity.Infrastructure.Persistence;
using HrAgencySystem.Identity.Projections;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.Identity.Application.Port;

public interface IAccountRepository
{
    Task<UserEmailReservation?> FindUserByEmail(Email email, string slug, CancellationToken ct);
    Task<OwnerEmailReservation?> FindOwnerByEmail(Email email, CancellationToken ct);
    Task<OwnerProjection> GetOwner(PlatformOwnerId id, CancellationToken ct);
    Task<UserProjection> GetUser(UserId userId, CancellationToken ct);
}