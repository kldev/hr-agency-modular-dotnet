using HrAgencySystem.Identity.Domain;
using HrAgencySystem.Identity.Events;
using HrAgencySystem.Identity.Services;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Time;
using Wolverine.Marten;

namespace HrAgencySystem.Identity.Application.Users.ChangeRole;

public static class ChangeRoleHandler
{
    public const string SameRoleMessage = "This person already has that role.";

    [AggregateHandler]
    public static async Task<(RoleChanged, Wolverine.Marten.Events)> Handle(
        ChangeRole command,
        User aggregate,
        IIdentityService service,
        IClock clock,
        CancellationToken ct
    )
    {
        service.ValidateAggregateUpdate(aggregate, command.OrganizationId.Value);

        if (aggregate.Role == command.Role)
            throw new BusinessRuleException(SameRoleMessage);

        var user =
            command.ModifiedBy == Guid.Empty
                ? UserSnapshot.System
                : await service.GetUserAsync(command.ModifiedBy, ct);

        var @event = new RoleChanged(
            command.UserId,
            aggregate.OrganizationId.Value,
            aggregate.Role,
            command.Role,
            user,
            clock.UtcNow
        );

        return (@event, [@event]);
    }
}
