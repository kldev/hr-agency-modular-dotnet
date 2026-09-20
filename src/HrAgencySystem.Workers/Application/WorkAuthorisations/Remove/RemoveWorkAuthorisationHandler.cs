using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.Workers.Domain;
using HrAgencySystem.Workers.Events;
using HrAgencySystem.Workers.Services;
using Wolverine.Marten;

namespace HrAgencySystem.Workers.Application.WorkAuthorisations.Remove;

public static class RemoveWorkAuthorisationHandler
{
    public const string UnknownAuthorisationMessage =
        "That permission is not on this person's file.";

    [AggregateHandler]
    public static async Task<(WorkAuthorisationRemoved, Wolverine.Marten.Events)> Handle(
        RemoveWorkAuthorisation command,
        Worker aggregate,
        IWorkersService service,
        IClock clock,
        CancellationToken ct
    )
    {
        ArgumentNullException.ThrowIfNull(aggregate);
        service.ValidateAggregateUpdate(aggregate, command.OrganizationId);

        if (aggregate.AuthorisationById(command.AuthorisationId) is null)
            throw new BusinessRuleException(UnknownAuthorisationMessage);

        var user = await service.GetUserAsync(command.ModifiedBy, ct);

        var @event = new WorkAuthorisationRemoved(
            aggregate.Id.Value,
            aggregate.OrganizationId.Value,
            command.AuthorisationId,
            user,
            clock.UtcNow
        );

        return (@event, [@event]);
    }
}
