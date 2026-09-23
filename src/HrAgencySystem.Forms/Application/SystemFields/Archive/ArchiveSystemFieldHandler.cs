using HrAgencySystem.Forms.Domain.SystemFields;
using HrAgencySystem.Forms.Events;
using HrAgencySystem.Forms.Services;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Time;
using Wolverine.Marten;

namespace HrAgencySystem.Forms.Application.SystemFields.Archive;

/// <summary>
/// Takes a field out of the catalogue's offer. Its code stays taken and every value given for it
/// stays readable; a draft still holding it will not save or publish until the field is removed.
/// </summary>
public static class ArchiveSystemFieldHandler
{
    [AggregateHandler(ConcurrencyStyle.Exclusive)]
    public static async Task<(SystemFieldArchived, Wolverine.Marten.Events)> Handle(
        ArchiveSystemField command,
        SystemFieldCatalogue aggregate,
        IFormsService service,
        IClock clock,
        CancellationToken ct
    )
    {
        service.ValidateAggregateUpdate(aggregate, command.OrganizationId, "System field", command.SystemFieldId);

        var field =
            aggregate.FieldById(command.SystemFieldId)
            ?? throw new NotFoundException("System field", command.SystemFieldId);

        if (field.IsArchived)
            throw new BusinessRuleException(SystemFieldRules.ArchivedMessage);

        var user = await service.GetUserAsync(command.ModifiedBy, ct);

        var @event = new SystemFieldArchived(command.OrganizationId, field.SystemFieldId, user, clock.UtcNow);

        return (@event, [@event]);
    }
}
