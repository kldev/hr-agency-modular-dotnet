using HrAgencySystem.Identity.Application.Port;
using HrAgencySystem.Identity.Domain.ValueObjects;
using HrAgencySystem.SharedKernel.Exception;

namespace HrAgencySystem.Identity.Application.Users.Avatar.Remove;

public static class RemoveUserAvatarHandler
{
    public const string NoAvatarMessage = "There is no picture to remove.";

    public static async Task<UserAvatarRemoved> Handle(
        RemoveUserAvatar command,
        IUserProfileRepository repository,
        CancellationToken ct
    )
    {
        var removed = await repository.RemoveAvatarAsync(
            command.OrganizationId,
            UserId.From(command.UserId),
            ct
        );

        if (removed is null)
            throw new BusinessRuleException(NoAvatarMessage);

        return new UserAvatarRemoved(command.UserId, removed.Value);
    }
}
