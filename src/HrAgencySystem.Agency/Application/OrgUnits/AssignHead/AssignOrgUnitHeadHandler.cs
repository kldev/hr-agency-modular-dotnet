using HrAgencySystem.Agency.Application.OrgUnits.Rename;
using HrAgencySystem.Agency.Domain;
using HrAgencySystem.Agency.Events;
using HrAgencySystem.Agency.Services;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Time;
using Wolverine.Marten;

namespace HrAgencySystem.Agency.Application.OrgUnits.AssignHead;

public static class AssignOrgUnitHeadHandler
{
    public const string NotAMemberMessage =
        "The head of a unit is one of its people. Put them in the unit first.";

    public const string AlreadyTheHeadMessage = "This person already heads this unit.";

    [AggregateHandler(ConcurrencyStyle.Exclusive)]
    public static async Task<(OrgUnitHeadAssigned, Wolverine.Marten.Events)> Handle(
        AssignOrgUnitHead command,
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

        if (unit.HeadUserId == command.HeadUserId)
            throw new BusinessRuleException(AlreadyTheHeadMessage);

        /*
         * The head belongs to the unit they head. That makes "the people of this unit" one
         * question instead of two, and it is what lets the supervisor walk stop at a head rather
         * than having to ask separately whether that head is even here. It also follows from one
         * person, one unit: somebody can head only the unit they sit in.
         */
        if (!unit.HasMember(command.HeadUserId))
            throw new BusinessRuleException(NotAMemberMessage);

        var modifiedBy = await service.GetUserAsync(command.ModifiedBy, ct);

        var @event = new OrgUnitHeadAssigned(
            aggregate.OrganizationId.Value,
            unit.UnitId,
            command.HeadUserId,
            modifiedBy,
            clock.UtcNow
        );

        return (@event, [@event]);
    }
}
