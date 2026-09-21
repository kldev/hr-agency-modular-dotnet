using HrAgencySystem.Agency.Application.OrgUnits.Create;
using HrAgencySystem.Agency.Domain;
using HrAgencySystem.Agency.Domain.ValueObjects;
using HrAgencySystem.Agency.Events;
using HrAgencySystem.Agency.Services;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Time;
using Wolverine.Marten;

namespace HrAgencySystem.Agency.Application.OrgUnits.Rename;

public static class RenameOrgUnitHandler
{
    public const string UnknownUnitMessage = "There is no such unit in this organization.";

    [AggregateHandler(ConcurrencyStyle.Exclusive)]
    public static async Task<(OrgUnitRenamed, Wolverine.Marten.Events)> Handle(
        RenameOrgUnit command,
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
            ?? throw new BusinessRuleException(UnknownUnitMessage);

        var (name, error) = OrgUnitName.TryCreate(command.Name);

        if (error is not null)
            throw new ValidationException(error);

        if (aggregate.HasSiblingNamed(unit.ParentId, name!.Value, unit.UnitId))
            throw new BusinessRuleException(CreateOrgUnitHandler.NameAlreadyUsedMessage);

        var modifiedBy = await service.GetUserAsync(command.ModifiedBy, ct);

        var @event = new OrgUnitRenamed(
            aggregate.OrganizationId.Value,
            unit.UnitId,
            name.Value,
            modifiedBy,
            clock.UtcNow
        );

        return (@event, [@event]);
    }
}
