using HrAgencySystem.LegalEntities.Application.Create;
using HrAgencySystem.LegalEntities.Domain;
using HrAgencySystem.LegalEntities.Events;
using HrAgencySystem.LegalEntities.Services;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Time;
using Wolverine.Marten;

namespace HrAgencySystem.LegalEntities.Application.Close;

public static class CloseLegalEntityHandler
{
    public const string AlreadyClosedMessage = "This legal entity is already closed.";

    [AggregateHandler]
    public static async Task<(LegalEntityClosed, Wolverine.Marten.Events)> Handle(
        CloseLegalEntity command,
        LegalEntity aggregate,
        ILegalEntitiesService service,
        IClock clock,
        CancellationToken ct
    )
    {
        service.ValidateAggregateUpdate(aggregate, command.OrganizationId);

        if (aggregate.ActiveTo is not null)
            throw new BusinessRuleException(AlreadyClosedMessage);

        if (command.ActiveTo < aggregate.ActiveFrom)
            throw new BusinessRuleException(LegalEntityDataFactory.ClosedBeforeOpenedMessage);

        var closedBy = await service.GetUserAsync(command.ModifiedBy, ct);

        var @event = new LegalEntityClosed(
            command.LegalEntityId,
            aggregate.OrganizationId.Value,
            command.ActiveTo,
            closedBy,
            clock.UtcNow
        );

        return (@event, [@event]);
    }
}
