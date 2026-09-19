using HrAgencySystem.Identity.Application.Port;
using HrAgencySystem.Identity.Infrastructure.IAM;

namespace HrAgencySystem.Identity.Application.Users.Logout;

public static class LogoutUserHandler
{
    public static async Task Handle(
        LogoutUser command,
        IRefreshTokenRepository refreshTokens,
        CancellationToken ct
    )
    {
        var stored = await refreshTokens.FindByHashAsync(
            SecureToken.Hash(command.RefreshToken),
            ct
        );

        // Logging out twice, or with a token that was already rotated away, is not a failure - and
        // saying so would tell an attacker which tokens exist.
        if (stored is null)
            return;

        // The whole family goes, not just this one: a rotated-away token from the same login would
        // otherwise still be exchangeable after the user signed out.
        await refreshTokens.RevokeFamilyAsync(stored.FamilyId, ct);
    }
}
