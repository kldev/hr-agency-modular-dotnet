using HrAgencySystem.EmailTemplates.Contracts.Identity;
using HrAgencySystem.Identity.Application.Policy;
using HrAgencySystem.Identity.Application.Port;
using HrAgencySystem.Identity.Events;
using HrAgencySystem.Identity.Infrastructure.IAM;
using HrAgencySystem.Identity.Services;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Time;
using Marten;
using Wolverine;
using OrganizationIdentity = HrAgencySystem.SharedKernel.Tenant.OrganizationId;
using UserIdentity = HrAgencySystem.Identity.Domain.ValueObjects.UserId;

namespace HrAgencySystem.Identity.Sagas;

/// <summary>
/// The reset window as a long running process: it opens when somebody asks for a link, it closes
/// either because the password was changed or because the timeout fired. Nothing else has to
/// remember when a link stops working - the saga simply stops existing.
/// </summary>
public sealed class PasswordResetSaga : Saga
{
    public const string InvalidTokenMessage = "The password reset link is invalid or has expired.";

    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid OrganizationId { get; set; }

    /// Only the hash: a leaked saga table still cannot be used to set anybody's password.
    public string TokenHash { get; set; } = "";

    public DateTimeOffset ExpiresAt { get; set; }

    public static (PasswordResetSaga, SendPasswordReset, PasswordResetExpired) Start(
        StartPasswordReset message,
        IClock clock
    )
    {
        var window = TimeSpan.FromMinutes(message.ExpiresInMinutes);

        var saga = new PasswordResetSaga
        {
            Id = message.ResetId,
            UserId = message.UserId,
            OrganizationId = message.OrganizationId,
            TokenHash = message.TokenHash,
            ExpiresAt = clock.UtcNow.Add(window),
        };

        var mail = new SendPasswordReset(
            Guid.NewGuid(),
            "identity",
            message.RecipientFullname,
            message.RecipientEmail,
            message.ResetUrl,
            message.ExpiresInMinutes
        );

        return (saga, mail, new PasswordResetExpired(message.ResetId, window));
    }

    /// <summary>
    /// The window closed on its own. Completing the saga is what makes the link stop working -
    /// there is no expiry column anybody could forget to check.
    /// </summary>
    public void Handle(PasswordResetExpired _) => MarkCompleted();

    public async Task Handle(
        CompletePasswordReset message,
        IIdentityService identity,
        IUserEmailReservationRepository reservations,
        IRefreshTokenRepository refreshTokens,
        IPasswordHasher hasher,
        IDocumentSession session,
        IClock clock,
        CancellationToken ct
    )
    {
        // A redelivered timeout and a redeemed link can race; the saga is still alive here, so the
        // window is checked rather than assumed.
        if (clock.UtcNow > ExpiresAt || Hash(message.Token) != TokenHash)
            throw new BusinessRuleException(InvalidTokenMessage);

        PasswordPolicyValidator.Validate(message.NewPassword);

        var passwordHash = hasher.Hash(message.NewPassword);

        await reservations.ChangePasswordAsync(
            OrganizationIdentity.From(OrganizationId),
            UserIdentity.From(UserId),
            passwordHash,
            ct
        );

        // Whoever forced this reset is the reason the password is being changed, so every session
        // opened before it dies with it - the old refresh tokens would outlive the password by weeks.
        await refreshTokens.RevokeUserSessionsAsync(
            OrganizationIdentity.From(OrganizationId),
            UserIdentity.From(UserId),
            ct
        );

        var user = await identity.GetUserAsync(UserId, ct);

        session.Events.Append(
            UserId,
            new PasswordChanged(UserId, OrganizationId, passwordHash, user, clock.UtcNow)
        );

        MarkCompleted();
    }

    public static string Hash(string token) => SecureToken.Hash(token);
}
