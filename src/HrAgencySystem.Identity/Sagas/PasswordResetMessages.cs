using Wolverine;
using Wolverine.Persistence.Sagas;

namespace HrAgencySystem.Identity.Sagas;

/// <summary>
/// Opens the reset window. Carries everything the mail needs, because the saga is the only thing
/// that knows the window is open and nobody else should have to read its state to send the link.
/// </summary>
public sealed record StartPasswordReset(
    [property: SagaIdentity] Guid ResetId,
    Guid UserId,
    Guid OrganizationId,
    string RecipientEmail,
    string RecipientFullname,
    string TokenHash,
    string ResetUrl,
    int ExpiresInMinutes
);

/// <summary>
/// Scheduled by the saga at its own start and delivered when the window closes. Wolverine stamps
/// the saga identity on a timeout cascaded from a saga, so this only has to say which reset it is.
/// </summary>
public sealed record PasswordResetExpired([property: SagaIdentity] Guid ResetId, TimeSpan DelayTime)
    : TimeoutMessage(DelayTime);

/// <summary>
/// Redeems the link from the mail. The raw token never touches storage, so it is checked here
/// against the hash the saga is holding.
/// </summary>
public sealed record CompletePasswordReset(
    [property: SagaIdentity] Guid ResetId,
    string Token,
    string NewPassword
);
