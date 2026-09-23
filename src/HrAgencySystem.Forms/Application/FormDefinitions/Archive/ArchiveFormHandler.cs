using HrAgencySystem.Forms.Domain;
using HrAgencySystem.Forms.Events;
using HrAgencySystem.Forms.Services;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Time;
using Wolverine.Marten;

namespace HrAgencySystem.Forms.Application.FormDefinitions.Archive;

/// <summary>
/// Closes a form to new responses. Nothing is deleted and nothing given stops being readable;
/// responses already started may still be submitted, since they are bound to their own version.
/// There is no way back - an archived form is a fact, like a completed project.
/// </summary>
public static class ArchiveFormHandler
{
    public const string AlreadyArchivedMessage = "This form is already archived.";

    [AggregateHandler]
    public static async Task<(FormArchived, Wolverine.Marten.Events)> Handle(
        ArchiveForm command,
        FormDefinition aggregate,
        IFormsService service,
        IClock clock,
        CancellationToken ct
    )
    {
        service.ValidateAggregateUpdate(aggregate, command.OrganizationId, "Form", command.FormId);

        if (aggregate.IsArchived)
            throw new BusinessRuleException(AlreadyArchivedMessage);

        var user = await service.GetUserAsync(command.ModifiedBy, ct);

        var @event = new FormArchived(command.OrganizationId, command.FormId, user, clock.UtcNow);

        return (@event, [@event]);
    }
}
