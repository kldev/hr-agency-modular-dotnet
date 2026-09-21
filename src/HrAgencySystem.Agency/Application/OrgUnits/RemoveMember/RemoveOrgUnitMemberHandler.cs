using HrAgencySystem.Agency.Application.OrgUnits.Rename;
using HrAgencySystem.Agency.Domain;
using HrAgencySystem.Agency.Events;
using HrAgencySystem.Agency.Services;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Time;
using Wolverine.Marten;

namespace HrAgencySystem.Agency.Application.OrgUnits.RemoveMember;

public static class RemoveOrgUnitMemberHandler
{
    public const string NotHereMessage = "This person is not in this unit.";

    public const string IsTheHeadMessage =
        "This person heads this unit. Name another head, or clear it, before taking them out.";

    [AggregateHandler(ConcurrencyStyle.Exclusive)]
    public static async Task<(OrgUnitMemberRemoved, Wolverine.Marten.Events)> Handle(
        RemoveOrgUnitMember command,
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

        if (!unit.HasMember(command.UserId))
            throw new BusinessRuleException(NotHereMessage);

        // A head is a member of the unit they head, so taking them out silently would leave a head
        // who is not there. Two facts, two commands - the caller says which one they meant.
        if (unit.HeadUserId == command.UserId)
            throw new BusinessRuleException(IsTheHeadMessage);

        var modifiedBy = await service.GetUserAsync(command.ModifiedBy, ct);

        var @event = new OrgUnitMemberRemoved(
            aggregate.OrganizationId.Value,
            unit.UnitId,
            command.UserId,
            modifiedBy,
            clock.UtcNow
        );

        return (@event, [@event]);
    }
}
