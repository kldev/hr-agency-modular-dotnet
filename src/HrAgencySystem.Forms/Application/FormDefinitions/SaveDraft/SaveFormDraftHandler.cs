using HrAgencySystem.Forms.Application.FormDefinitions.UpdateDetails;
using HrAgencySystem.Forms.Application.Port;
using HrAgencySystem.Forms.Domain;
using HrAgencySystem.Forms.Domain.Layout;
using HrAgencySystem.Forms.Events;
using HrAgencySystem.Forms.Services;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Time;
using Wolverine.Marten;

namespace HrAgencySystem.Forms.Application.FormDefinitions.SaveDraft;

public static class SaveFormDraftHandler
{
    [AggregateHandler]
    public static async Task<(FormDraftSaved, Wolverine.Marten.Events)> Handle(
        SaveFormDraft command,
        FormDefinition aggregate,
        IFormsService service,
        IFormsRepository repository,
        IClock clock,
        CancellationToken ct
    )
    {
        service.ValidateAggregateUpdate(aggregate, command.OrganizationId, "Form", command.FormId);

        if (aggregate.IsArchived)
            throw new BusinessRuleException(UpdateFormDetailsHandler.ArchivedMessage);

        var catalogue = await repository.GetCatalogueAsync(command.OrganizationId, ct);

        // System fields are refreshed from today's catalogue on every save; only publishing freezes them.
        var (pages, errors) = FormLayoutPolicy.PrepareDraft(command.Pages, catalogue);

        if (errors.Count > 0)
            throw FormErrors.From(errors);

        var user = await service.GetUserAsync(command.ModifiedBy, ct);

        var @event = new FormDraftSaved(command.OrganizationId, command.FormId, pages, user, clock.UtcNow);

        return (@event, [@event]);
    }
}
