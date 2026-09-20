using HrAgencySystem.Identity.Application.Port;
using HrAgencySystem.Identity.Documents;
using HrAgencySystem.Identity.Domain.ValueObjects;
using HrAgencySystem.SharedKernel.Time;

namespace HrAgencySystem.Identity.Application.Users.Avatar.Change;

public static class ChangeUserAvatarHandler
{
    public static async Task<UserAvatarChanged> Handle(
        ChangeUserAvatar command,
        IUserProfileRepository repository,
        IClock clock,
        CancellationToken ct
    )
    {
        var profile = new UserProfile(
            command.UserId,
            command.OrganizationId.Value,
            command.FileId,
            command.FileName,
            command.ContentType,
            command.Size,
            clock.UtcNow
        );

        var previousFileId = await repository.SetAvatarAsync(profile, ct);

        return new UserAvatarChanged(command.UserId, command.FileId, previousFileId);
    }
}
