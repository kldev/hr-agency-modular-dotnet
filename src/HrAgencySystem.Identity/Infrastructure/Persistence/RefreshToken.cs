using HrAgencySystem.Identity.Infrastructure.IAM;
using HrAgencySystem.SharedKernel.Time;

namespace HrAgencySystem.Identity.Infrastructure.Persistence;

/// <summary>
/// One issued refresh token. A login opens a family; every rotation adds a member to it and leaves
/// the spent one behind with <see cref="UsedAt"/> set, because a token that no longer exists cannot
/// tell us it was replayed. Only the hash is stored - a leaked table cannot be turned into a session.
/// </summary>
public sealed record RefreshToken(
    Guid Id,
    Guid FamilyId,
    Guid UserId,
    Guid OrganizationId,
    string TokenHash,
    DateTimeOffset IssuedAt,
    DateTimeOffset ExpiresAt,
    DateTimeOffset? UsedAt = null,
    Guid? ReplacedById = null
)
{
    public static (RefreshToken Token, string Value) Issue(
        Guid userId,
        Guid organizationId,
        IClock clock,
        int expiresInDays
    )
    {
        var value = SecureToken.New();
        var now = clock.UtcNow;
        var id = Guid.NewGuid();

        var token = new RefreshToken(
            id,
            FamilyId: id,
            userId,
            organizationId,
            SecureToken.Hash(value),
            now,
            now.AddDays(expiresInDays)
        );

        return (token, value);
    }

    /// <summary>
    /// The replacement inherits <see cref="ExpiresAt"/>, so the window is counted from the login and
    /// from nowhere else. Rotation can therefore never extend a session past its thirty days.
    /// </summary>
    public (RefreshToken Token, string Value) Rotate(IClock clock)
    {
        var value = SecureToken.New();

        var token = new RefreshToken(
            Guid.NewGuid(),
            FamilyId,
            UserId,
            OrganizationId,
            SecureToken.Hash(value),
            clock.UtcNow,
            ExpiresAt
        );

        return (token, value);
    }

    public RefreshToken SpentOn(RefreshToken replacement, IClock clock) =>
        this with
        {
            UsedAt = clock.UtcNow,
            ReplacedById = replacement.Id,
        };

    public bool WasAlreadyUsed => UsedAt is not null;

    public bool HasExpired(IClock clock) => clock.UtcNow >= ExpiresAt;
}
