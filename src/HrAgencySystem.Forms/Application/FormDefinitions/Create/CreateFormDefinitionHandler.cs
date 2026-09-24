using HrAgencySystem.Forms.Application.Port;
using HrAgencySystem.Forms.Domain;
using HrAgencySystem.Forms.Domain.ValueObjects;
using HrAgencySystem.Forms.Events;
using HrAgencySystem.Forms.Services;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Time;
using Marten;

namespace HrAgencySystem.Forms.Application.FormDefinitions.Create;

public static class CreateFormDefinitionHandler
{
    public const string CodeTakenMessage = "Another form in this organization already uses this code.";

    public const string DescriptionTooLongMessage = "A description cannot exceed 2000 characters.";

    public static async Task<FormDefinitionCreated> Handle(
        CreateFormDefinition command,
        IFormsService service,
        IFormCodeReservationRepository reservations,
        IDocumentSession session,
        IClock clock,
        CancellationToken ct
    )
    {
        await service.ValidateOrganization(command.OrganizationId, ct);

        var errors = new List<string>();

        var (code, codeError) = FormCode.TryCreate(command.Code);
        if (codeError is not null)
            errors.Add(codeError);

        var (name, nameError) = FormName.TryCreate(command.Name);
        if (nameError is not null)
            errors.Add(nameError);

        var description = string.IsNullOrWhiteSpace(command.Description) ? null : command.Description.Trim();
        if (description is { Length: > 2000 })
            errors.Add(DescriptionTooLongMessage);

        if (!SubjectKinds.IsKnown(command.SubjectKind))
            errors.Add(SubjectKinds.UnknownKindMessage);

        if (errors.Count > 0)
            throw new ValidationException(errors);

        // The friendly answer; the unique index on the reservation is what defeats a concurrent twin.
        if (await reservations.ExistsAsync(command.OrganizationId, code!.Value, ct))
            throw new BusinessRuleException(CodeTakenMessage);

        var createdBy = await service.GetUserAsync(command.CreatedBy, ct);

        var @event = new FormDefinitionCreated(
            command.OrganizationId,
            Guid.CreateVersion7(),
            code.Value,
            name!.Value,
            description,
            command.Kind,
            command.Cardinality ?? DefaultCardinality(command.Kind),
            command.SubjectKind,
            createdBy,
            clock.UtcNow
        );

        session.Events.StartStream<FormDefinition>(@event.FormId, @event);
        reservations.Reserve(command.OrganizationId, code.Value, @event.FormId);

        return @event;
    }

    public static ResponseCardinality DefaultCardinality(FormKind kind) =>
        kind == FormKind.Survey ? ResponseCardinality.Many : ResponseCardinality.OnePerSubject;
}
