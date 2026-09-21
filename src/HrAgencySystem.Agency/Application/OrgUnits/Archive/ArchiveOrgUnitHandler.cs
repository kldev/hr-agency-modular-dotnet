using HrAgencySystem.Agency.Application.OrgUnits.Rename;
using HrAgencySystem.Agency.Domain;
using HrAgencySystem.Agency.Events;
using HrAgencySystem.Agency.Services;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Time;
using Wolverine.Marten;

namespace HrAgencySystem.Agency.Application.OrgUnits.Archive;

/// <summary>
/// Closes a unit. Archived rather than deleted, because a department that was dissolved still
/// stands next to the leave somebody approved in it last year.
/// </summary>
public static class ArchiveOrgUnitHandler
{
    public const string RootCannotBeArchivedMessage =
        "The top unit stays. Archiving it would leave the chart with no top.";

    public const string StillHasPeopleMessage =
        "This unit still has people in it. Move them somewhere else first.";

    public const string StillHasUnitsMessage =
        "This unit still has units under it. Move or archive those first.";

    public const string AlreadyArchivedMessage = "This unit is already archived.";

    [AggregateHandler(ConcurrencyStyle.Exclusive)]
    public static async Task<(OrgUnitArchived, Wolverine.Marten.Events)> Handle(
        ArchiveOrgUnit command,
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

        if (unit.IsArchived)
            throw new BusinessRuleException(AlreadyArchivedMessage);

        if (unit.IsRoot)
            throw new BusinessRuleException(RootCannotBeArchivedMessage);

        /*
         * Emptied first, on purpose. A unit archived with people still in it would keep answering
         * "who is their supervisor" from a box the company no longer has - and nobody would see it,
         * because the archived unit is gone from every list.
         */
        if (unit.Members.Count > 0)
            throw new BusinessRuleException(StillHasPeopleMessage);

        if (aggregate.ChildrenOf(unit.UnitId).Count > 0)
            throw new BusinessRuleException(StillHasUnitsMessage);

        var modifiedBy = await service.GetUserAsync(command.ModifiedBy, ct);

        var @event = new OrgUnitArchived(
            aggregate.OrganizationId.Value,
            unit.UnitId,
            modifiedBy,
            clock.UtcNow
        );

        return (@event, [@event]);
    }
}
