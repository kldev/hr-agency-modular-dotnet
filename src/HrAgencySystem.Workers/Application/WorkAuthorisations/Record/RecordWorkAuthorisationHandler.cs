using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.SharedKernel.ValueObjects;
using HrAgencySystem.Workers.Domain;
using HrAgencySystem.Workers.Domain.ValueObjects;
using HrAgencySystem.Workers.Events;
using HrAgencySystem.Workers.Services;
using Wolverine.Marten;

namespace HrAgencySystem.Workers.Application.WorkAuthorisations.Record;

public static class RecordWorkAuthorisationHandler
{
    public const string NotRequiredMessage =
        "This person holds free movement rights and needs no permission to work.";

    public const string UnknownDocumentMessage =
        "The referenced document is not on this person's file.";

    public const string UnknownAuthorisationMessage =
        "That permission is not on this person's file.";

    [AggregateHandler]
    public static async Task<(WorkAuthorisationRecorded, Wolverine.Marten.Events)> Handle(
        RecordWorkAuthorisation command,
        Worker aggregate,
        IWorkersService service,
        IClock clock,
        CancellationToken ct
    )
    {
        ArgumentNullException.ThrowIfNull(aggregate);
        service.ValidateAggregateUpdate(aggregate, command.OrganizationId);

        // Recording a work permit for somebody who does not need one is not a harmless extra row:
        // it is a claim that this person's right to work depends on a piece of paper, and the next
        // reader will act on it.
        if (!aggregate.RequiresLegalisation)
            throw new BusinessRuleException(NotRequiredMessage);

        if (
            command.AuthorisationId is { } existingId
            && aggregate.AuthorisationById(existingId) is null
        )
            throw new BusinessRuleException(UnknownAuthorisationMessage);

        if (
            command.DocumentId is not null
            && aggregate.DocumentById(command.DocumentId.Value) is null
        )
            throw new BusinessRuleException(UnknownDocumentMessage);

        var authorisation = Build(command);
        var user = await service.GetUserAsync(command.ModifiedBy, ct);

        var @event = new WorkAuthorisationRecorded(
            aggregate.Id.Value,
            aggregate.OrganizationId.Value,
            authorisation,
            user,
            clock.UtcNow
        );

        return (@event, [@event]);
    }

    private static WorkAuthorisation Build(RecordWorkAuthorisation command)
    {
        var errors = new List<string>();

        var (country, countryError) = CountryCode.TryCreate(command.Country);
        if (countryError is not null)
            errors.Add(countryError);

        var (number, numberError) = DocumentNumber.TryCreate(command.Number);
        if (numberError is not null)
            errors.Add(numberError);

        if (command.ValidUntil < command.ValidFrom)
            errors.Add(WorkAuthorisation.ValidUntilBeforeValidFromMessage);

        var (note, noteError) = ShortNote.TryCreate(command.Note ?? "", false);
        if (noteError is not null)
            errors.Add(noteError);

        if (errors.Count > 0)
            throw new ValidationException(errors);

        return new WorkAuthorisation(
            command.AuthorisationId ?? Guid.NewGuid(),
            command.Kind,
            country!.Value,
            number!.Value,
            command.ValidFrom,
            command.ValidUntil,
            command.DocumentId,
            string.IsNullOrWhiteSpace(note?.Value) ? null : note.Value
        );
    }
}
