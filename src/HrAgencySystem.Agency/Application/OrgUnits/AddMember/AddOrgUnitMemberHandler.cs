using HrAgencySystem.Agency.Application.OrgUnits.Rename;
using HrAgencySystem.Agency.Domain;
using HrAgencySystem.Agency.Events;
using HrAgencySystem.Agency.Services;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.SharedKernel.ValueObjects;
using Wolverine.Marten;

namespace HrAgencySystem.Agency.Application.OrgUnits.AddMember;

public static class AddOrgUnitMemberHandler
{
    public const string UnitArchivedMessage = "That unit is archived, so nobody new goes into it.";

    public const string AlreadyInAnotherUnitMessage =
        "This person already belongs to another unit. Move them instead of adding them twice.";

    public const string AlreadyHereMessage = "This person is already in this unit.";

    [AggregateHandler(ConcurrencyStyle.Exclusive)]
    public static async Task<(OrgUnitMemberAdded, Wolverine.Marten.Events)> Handle(
        AddOrgUnitMember command,
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
            throw new BusinessRuleException(UnitArchivedMessage);

        if (unit.HasMember(command.UserId))
            throw new BusinessRuleException(AlreadyHereMessage);

        /*
         * One person, one unit - checked here rather than by a reservation document, because the
         * whole chart is this one aggregate and this one transaction. Somebody working for two
         * departments still has one supervisor, and that is the right answer rather than a gap.
         */
        if (aggregate.UnitOfAnother(command.UserId, unit.UnitId) is not null)
            throw new BusinessRuleException(AlreadyInAnotherUnitMessage);

        var organizationId = OrganizationId.From(command.OrganizationId);
        var member = await service.GetOrganizationMemberAsync(organizationId, command.UserId, ct);
        var modifiedBy = await service.GetUserAsync(command.ModifiedBy, ct);

        var title = ReadTitle(command.Title);

        var @event = new OrgUnitMemberAdded(
            aggregate.OrganizationId.Value,
            unit.UnitId,
            new OrgUnitMember(member.Id, title),
            modifiedBy,
            clock.UtcNow
        );

        return (@event, [@event]);
    }

    /// <summary>
    /// Optional, and only earns its place in the case the chart cannot express: the second owner
    /// sits on the board with a title of their own while the other one heads it.
    /// </summary>
    private static string ReadTitle(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "";

        var (title, error) = ShortNote.TryCreate(value);

        return error is not null ? throw new ValidationException(error) : title!.Value;
    }
}
