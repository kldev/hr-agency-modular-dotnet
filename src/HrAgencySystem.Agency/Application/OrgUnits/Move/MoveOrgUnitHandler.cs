using HrAgencySystem.Agency.Application.OrgUnits.Create;
using HrAgencySystem.Agency.Application.OrgUnits.Rename;
using HrAgencySystem.Agency.Domain;
using HrAgencySystem.Agency.Events;
using HrAgencySystem.Agency.Services;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Time;
using Wolverine.Marten;

namespace HrAgencySystem.Agency.Application.OrgUnits.Move;

/// <summary>
/// Hangs a unit under a different parent, with its people and its head. Reorganising a company is
/// moving boxes, not rebuilding them - and the supervisor of everyone inside follows on its own,
/// which is the whole point of computing it instead of writing it down.
/// </summary>
public static class MoveOrgUnitHandler
{
    public const string RootCannotMoveMessage = "The top unit has nothing to hang under.";

    public const string IntoItselfMessage = "A unit cannot be moved under itself.";

    public const string IntoOwnSubtreeMessage =
        "A unit cannot be moved under one of its own units - that would leave a loop with no top.";

    [AggregateHandler(ConcurrencyStyle.Exclusive)]
    public static async Task<(OrgUnitMoved, Wolverine.Marten.Events)> Handle(
        MoveOrgUnit command,
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

        if (unit.IsRoot)
            throw new BusinessRuleException(RootCannotMoveMessage);

        if (command.ParentId == command.UnitId)
            throw new BusinessRuleException(IntoItselfMessage);

        var parent =
            aggregate.UnitById(command.ParentId)
            ?? throw new BusinessRuleException(CreateOrgUnitHandler.UnknownParentMessage);

        if (parent.IsArchived)
            throw new BusinessRuleException(CreateOrgUnitHandler.ParentArchivedMessage);

        // The one check a tree of parent pointers cannot do without: a unit moved under its own
        // descendant makes a ring, and the supervisor walk would then never reach a top.
        if (SupervisorPolicy.Descendants(aggregate.Units, unit).Any(d => d.UnitId == parent.UnitId))
            throw new BusinessRuleException(IntoOwnSubtreeMessage);

        if (aggregate.HasSiblingNamed(parent.UnitId, unit.Name, unit.UnitId))
            throw new BusinessRuleException(CreateOrgUnitHandler.NameAlreadyUsedMessage);

        var modifiedBy = await service.GetUserAsync(command.ModifiedBy, ct);

        var @event = new OrgUnitMoved(
            aggregate.OrganizationId.Value,
            unit.UnitId,
            parent.UnitId,
            modifiedBy,
            clock.UtcNow
        );

        return (@event, [@event]);
    }
}
