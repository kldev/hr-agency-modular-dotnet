using HrAgencySystem.Agency.Application.OrgUnits.Rename;
using HrAgencySystem.Agency.Domain;
using HrAgencySystem.Agency.Events;
using HrAgencySystem.Agency.Services;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Time;
using Wolverine.Marten;

namespace HrAgencySystem.Agency.Application.OrgUnits.ClearHead;

/// <summary>
/// Leaves a unit without a head of its own. Not a gap to be filled: "Operations Poland" and
/// "Operations abroad" often have none, and the head of operations answers for both.
/// </summary>
public static class ClearOrgUnitHeadHandler
{
    public const string NoHeadMessage = "This unit has no head to clear.";

    [AggregateHandler(ConcurrencyStyle.Exclusive)]
    public static async Task<(OrgUnitHeadCleared, Wolverine.Marten.Events)> Handle(
        ClearOrgUnitHead command,
        OrgStructure aggregate,
        IAgencyService service,
        IClock clock,
        CancellationToken ct
    )
    {
        ArgumentNullException.ThrowIfNull(aggregate);
        service.ValidateAggregateUpdate(aggregate, command.OrganizationId);

        var unit =
            aggregate.UnitById(command.UnitId)
            ?? throw new BusinessRuleException(RenameOrgUnitHandler.UnknownUnitMessage);

        if (unit.HeadUserId is null)
            throw new BusinessRuleException(NoHeadMessage);

        var modifiedBy = await service.GetUserAsync(command.ModifiedBy, ct);

        var @event = new OrgUnitHeadCleared(
            aggregate.OrganizationId.Value,
            unit.UnitId,
            modifiedBy,
            clock.UtcNow
        );

        return (@event, [@event]);
    }
}
