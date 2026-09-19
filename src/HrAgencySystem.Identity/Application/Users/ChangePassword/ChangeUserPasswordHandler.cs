using HrAgencySystem.Identity.Application.Policy;
using HrAgencySystem.Identity.Application.Port;
using HrAgencySystem.Identity.Domain;
using HrAgencySystem.Identity.Events;
using HrAgencySystem.Identity.Services;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Time;
using Wolverine.Marten;

namespace HrAgencySystem.Identity.Application.Users.ChangePassword;

public static class ChangeUserPasswordHandler
{
    public const string InvalidCurrentPasswordMessage = "Current password is invalid.";

    public const string SamePasswordMessage = "New password must differ from the current one.";

    [AggregateHandler]
    public static async Task<(PasswordChanged, Wolverine.Marten.Events)> Handle(
        ChangeUserPassword command,
        User aggregate,
        IIdentityService service,
        IUserEmailReservationRepository repository,
        IPasswordHasher hasher,
        IClock clock,
        CancellationToken ct
    )
    {
        service.ValidateAggregateUpdate(aggregate, command.OrganizationId.Value);

        if (!hasher.Matches(command.CurrentPassword, aggregate.PasswordHash))
            throw new BusinessRuleException(InvalidCurrentPasswordMessage);

        PasswordPolicyValidator.Validate(command.NewPassword);

        if (hasher.Matches(command.NewPassword, aggregate.PasswordHash))
            throw new BusinessRuleException(SamePasswordMessage);

        var passwordHash = hasher.Hash(command.NewPassword);

        await repository.ChangePasswordAsync(
            aggregate.OrganizationId,
            aggregate.Id,
            passwordHash,
            ct
        );

        var user = await service.GetUserAsync(command.ModifiedBy, ct);

        var @event = new PasswordChanged(
            command.UserId,
            aggregate.OrganizationId.Value,
            passwordHash,
            user,
            clock.UtcNow
        );

        return (@event, [@event]);
    }
}
