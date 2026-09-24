using HrAgencySystem.Forms.Application.FormDefinitions.UpdateDetails;
using HrAgencySystem.Forms.Application.Port;
using HrAgencySystem.Forms.Documents;
using HrAgencySystem.Forms.Domain;
using HrAgencySystem.Forms.Domain.Layout;
using HrAgencySystem.Forms.Events;
using HrAgencySystem.Forms.Services;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Time;
using Wolverine.Marten;

namespace HrAgencySystem.Forms.Application.FormDefinitions.Publish;

/// <summary>
/// Freezes the draft as the next version.
/// <para>
/// System fields are resolved against the catalogue <em>once more</em>, here: a label changed in the
/// catalogue since the draft was last saved goes out with this version, and from this moment on the
/// version keeps its own copy - a later change in the catalogue never reaches it. Do not "simplify"
/// this into publishing the stored draft as is; that is what keeps a version and the catalogue apart.
/// </para>
/// <para>
/// The <see cref="FormVersion"/> document is written in the same transaction as the event, so a
/// response can be started against it the moment this returns.
/// </para>
/// </summary>
public static class PublishFormHandler
{
    public const string NothingToPublishMessage =
        "Nothing changed since the last published version. Change the draft first.";

    [AggregateHandler]
    public static async Task<(FormPublished, Wolverine.Marten.Events)> Handle(
        PublishForm command,
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

        if (aggregate.PublishedVersion > 0 && !aggregate.HasUnpublishedChanges)
            throw new BusinessRuleException(NothingToPublishMessage);

        var catalogue = await repository.GetCatalogueAsync(command.OrganizationId, ct);
        var (pages, errors) = FormLayoutPolicy.PrepareDraft(aggregate.Pages, catalogue);

        errors = [.. errors, .. FormLayoutPolicy.ValidatePublish(pages)];

        if (errors.Count > 0)
            throw FormErrors.From(errors);

        var user = await service.GetUserAsync(command.ModifiedBy, ct);
        var version = aggregate.PublishedVersion + 1;
        var now = clock.UtcNow;

        var @event = new FormPublished(command.OrganizationId, command.FormId, version, pages, user, now);

        repository.AddVersion(
            new FormVersion(
                FormsStreamId.ForVersion(command.FormId, version),
                command.OrganizationId,
                command.FormId,
                aggregate.Code,
                aggregate.Name,
                aggregate.Kind,
                version,
                pages,
                user,
                now
            )
        );

        return (@event, [@event]);
    }
}
