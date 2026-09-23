using HrAgencySystem.Forms.Domain.Layout;
using HrAgencySystem.Forms.Domain.SystemFields;
using HrAgencySystem.Forms.Events;
using HrAgencySystem.Forms.Services;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Time;
using Wolverine.Marten;

namespace HrAgencySystem.Forms.Application.SystemFields.Update;

/// <summary>
/// Changes the catalogue only. Drafts pick the change up the next time they are saved or previewed;
/// published versions never do - they keep the copy they went out with.
/// </summary>
public static class UpdateSystemFieldHandler
{
    [AggregateHandler(ConcurrencyStyle.Exclusive)]
    public static async Task<(SystemFieldUpdated, Wolverine.Marten.Events)> Handle(
        UpdateSystemField command,
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

        var label = (command.Label ?? "").Trim();
        var description = SystemFieldRules.Blank(command.Description);
        var (rules, options) = FieldRulesPolicy.Normalize(field.Type, command.Rules, command.Options);

        var errors = SystemFieldRules.Check(field.Type, label, description, rules, options, command.Source);

        if (errors.Count > 0)
            throw new ValidationException(errors);

        var user = await service.GetUserAsync(command.ModifiedBy, ct);

        var @event = new SystemFieldUpdated(
            command.OrganizationId,
            field.SystemFieldId,
            label,
            description,
            rules,
            options,
            command.Source,
            user,
            clock.UtcNow
        );

        return (@event, [@event]);
    }
}
