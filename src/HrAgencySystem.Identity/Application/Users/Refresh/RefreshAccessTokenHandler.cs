using HrAgencySystem.Identity.Application.Port;
using HrAgencySystem.Identity.Application.Users.Login;
using HrAgencySystem.Identity.Domain.ValueObjects;
using HrAgencySystem.Identity.Infrastructure.IAM;
using HrAgencySystem.Identity.Infrastructure.Persistence;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Time;
using Marten;
using Microsoft.Extensions.Logging;

namespace HrAgencySystem.Identity.Application.Users.Refresh;

public static class RefreshAccessTokenHandler
{
    public const string InvalidTokenMessage = "The session has expired. Please sign in again.";

    public static async Task<LoginUserResult> Handle(
        RefreshAccessToken command,
        ILogger logger,
        IRefreshTokenRepository refreshTokens,
        IAccountRepository accounts,
        IJwtTokenService tokenService,
        IDocumentSession session,
        IClock clock,
        CancellationToken ct
    )
    {
        var stored = await refreshTokens.FindByHashAsync(
            SecureToken.Hash(command.RefreshToken),
            ct
        );

        if (stored is null)
            throw new AuthorizationException(InvalidTokenMessage);

        if (stored.WasAlreadyUsed)
        {
            // This token was exchanged once already, so two parties are holding it: either the
            // client replayed it or it leaked. The session is not worth keeping either way.
            logger.LogWarning(
                "Refresh token replay detected for user {UserId}, revoking session family {FamilyId}.",
                stored.UserId,
                stored.FamilyId
            );

            throw await RevokeFamilyAndReject(stored, refreshTokens, session, ct);
        }

        if (stored.HasExpired(clock))
            throw await RevokeFamilyAndReject(stored, refreshTokens, session, ct);

        var (issued, value) = stored.Rotate(clock);

        await refreshTokens.RotateAsync(stored.SpentOn(issued, clock), issued, ct);

        var user = await accounts.GetUser(UserId.From(stored.UserId), ct);
        var access = tokenService.GenerateUserToken(user);

        return new LoginUserResult(access.Value, value, access.ExpiresAt, issued.ExpiresAt);
    }

    private static async Task<AuthorizationException> RevokeFamilyAndReject(
        RefreshToken stored,
        IRefreshTokenRepository refreshTokens,
        IDocumentSession session,
        CancellationToken ct
    )
    {
        await refreshTokens.RevokeFamilyAsync(stored.FamilyId, ct);

        // Rejecting is the whole point of this path, and the exception rolls the Wolverine
        // transaction back - so the revocation is committed here instead of being thrown away with it.
        await session.SaveChangesAsync(ct);

        return new AuthorizationException(InvalidTokenMessage);
    }
}
