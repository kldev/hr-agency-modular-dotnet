using HrAgencySystem.Identity.Application.Port;
using HrAgencySystem.Identity.Documents;
using HrAgencySystem.Identity.Domain.ValueObjects;
using HrAgencySystem.SharedKernel.Tenant;
using Marten;

namespace HrAgencySystem.Identity.Infrastructure.Persistence;

public sealed class UserProfileRepository(IDocumentSession session) : IUserProfileRepository
{
    public async Task<UserProfile?> GetAsync(
        OrganizationId organizationId,
        UserId userId,
        CancellationToken ct
    )
    {
        return await session
            .Query<UserProfile>()
            .Where(z => z.OrganizationId == organizationId.Value && z.Id == userId.Value)
            .SingleOrDefaultAsync(ct);
    }

    public async Task<Guid?> SetAvatarAsync(UserProfile profile, CancellationToken ct)
    {
        var current = await GetAsync(
            OrganizationId.From(profile.OrganizationId),
            UserId.From(profile.Id),
            ct
        );

        session.Store(profile);

        return current?.AvatarFileId;
    }

    public async Task<Guid?> RemoveAvatarAsync(
        OrganizationId organizationId,
        UserId userId,
        CancellationToken ct
    )
    {
        var current = await GetAsync(organizationId, userId, ct);

        if (current is null)
            return null;

        session.Delete(current);

        return current.AvatarFileId;
    }
}
