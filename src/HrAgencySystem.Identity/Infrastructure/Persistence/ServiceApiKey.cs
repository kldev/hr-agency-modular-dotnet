using HrAgencySystem.Identity.Infrastructure.IAM;
using HrAgencySystem.SharedKernel.Time;

namespace HrAgencySystem.Identity.Infrastructure.Persistence;

/// <summary>
/// A credential for a program rather than a person - the public job board, today. Issued by the
/// platform owner, long lived, and revoked by hand.
/// <para>
/// Belongs to no organization, and that is the point: the job board serves every agency and picks
/// the one it is talking about from the slug in each call. A key says "a program we know", never
/// "a tenant".
/// </para>
/// <para>
/// The value reads <c>sk_</c> followed by the random part, so anybody who finds it in a config file
/// or a paste knows what it is, and secret scanners can match it. <see cref="DisplayPrefix"/> keeps
/// the first few characters, enough to tell two keys apart in a list and nowhere near enough to use.
/// </para>
/// <para>
/// Only the hash is stored, like <see cref="RefreshToken"/>: the value exists once, in the answer
/// to issuing it, and a leaked table opens nothing. A revoked key keeps its row, so "what made that
/// call in March" still has an answer.
/// </para>
/// </summary>
public sealed record ServiceApiKey(
    Guid Id,
    string Name,
    string KeyHash,
    string DisplayPrefix,
    DateTimeOffset CreatedAt,
    Guid CreatedBy,
    DateTimeOffset? RevokedAt = null,
    Guid? RevokedBy = null
)
{
    public const string ValuePrefix = "sk_";

    /// <summary>How much of the value is kept in the clear: the prefix and eight random characters.</summary>
    private const int DisplayLength = 11;

    public const int NameMaxLength = 100;

    public const string NameRequiredMessage = "Say what the key is for, so it can be told apart later.";

    public static readonly string NameTooLongMessage =
        $"A key name cannot be longer than {NameMaxLength} characters.";

    public bool IsRevoked => RevokedAt is not null;

    /// <summary>
    /// A value that could not have come from <see cref="Issue"/>. Checked before hashing, so a user
    /// token pasted into the header is refused without a database round trip.
    /// </summary>
    public static bool LooksLikeKey(string? value) =>
        value is not null
        && value.StartsWith(ValuePrefix, StringComparison.Ordinal)
        && value.Length > ValuePrefix.Length;

    public static (ServiceApiKey Key, string Value) Issue(string name, Guid createdBy, IClock clock)
    {
        var value = ValuePrefix + SecureToken.New();

        var key = new ServiceApiKey(
            Guid.NewGuid(),
            name.Trim(),
            SecureToken.Hash(value),
            value[..DisplayLength],
            clock.UtcNow,
            createdBy
        );

        return (key, value);
    }

    public ServiceApiKey Revoke(Guid revokedBy, IClock clock) =>
        this with
        {
            RevokedAt = clock.UtcNow,
            RevokedBy = revokedBy,
        };
}
