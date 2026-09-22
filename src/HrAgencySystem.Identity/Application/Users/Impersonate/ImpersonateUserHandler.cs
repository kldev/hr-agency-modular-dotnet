using HrAgencySystem.Identity.Application.Port;
using HrAgencySystem.SharedKernel.Exception;
using Microsoft.Extensions.Logging;

namespace HrAgencySystem.Identity.Application.Users.Impersonate;

public static class ImpersonateUserHandler
{
    public const string CannotImpersonateSelfMessage =
        "You are already signed in as yourself. Pick somebody else.";

    /// <summary>
    /// The target is read through <see cref="IUserQueryRepository"/> and not through
    /// <c>IAccountRepository</c>, which looks a user up by id alone: that one is safe for its own
    /// callers only because they each resolved the id from an organization-scoped reservation first.
    /// This repository scopes by organization and drops the <c>System</c> role on the way, so a
    /// stranger's id, a made-up id and the platform's own account all answer the same "not found" -
    /// a refusal would confirm which of the three it was.
    /// </summary>
    public static async Task<ImpersonationResult> Handle(
        ImpersonateUser command,
        IUserQueryRepository users,
        IJwtTokenService tokenService,
        ILogger logger,
        CancellationToken ct
    )
    {
        if (command.TargetUserId == command.AdminUserId)
            throw new BusinessRuleException(CannotImpersonateSelfMessage);

        var target =
            await users.GetUser(command.OrganizationId, command.TargetUserId, ct)
            ?? throw new NotFoundException("User", command.TargetUserId);

        var access = tokenService.GenerateImpersonationToken(target, command.AdminUserId);

        // The whole audit trail. Worth a warning rather than information: somebody is about to act
        // under a name that is not theirs, and that is the line to find afterwards.
        logger.LogWarning(
            "Administrator {AdminUserId} signed in as user {TargetUserId} ({TargetEmail}) in organization {OrganizationId}",
            command.AdminUserId,
            target.Id,
            target.Email,
            command.OrganizationId.Value
        );

        return new ImpersonationResult(
            access.Value,
            access.ExpiresAt,
            target.Id,
            command.AdminUserId
        );
    }
}
