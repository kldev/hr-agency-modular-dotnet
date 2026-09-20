using HrAgencySystem.Projects.Domain;
using HrAgencySystem.Projects.Domain.Compliance;
using HrAgencySystem.Projects.Events;
using HrAgencySystem.Projects.Services;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.SharedKernel.ValueObjects;
using Wolverine.Marten;

namespace HrAgencySystem.Projects.Application.Compliance.Record;

public static class RecordComplianceItemHandler
{
    public const string NotInCatalogueMessage =
        "This requirement does not apply to the project's country and engagement type.";
    public const string ReferenceNumberRequiredMessage =
        "Confirming this requirement needs the reference number it was filed under.";
    public const string ValidToBeforeValidFromMessage =
        "The validity cannot end before it begins.";
    public const string UnknownDocumentMessage =
        "The referenced document is not attached to this project.";
    public const string ReferenceNumberMaxLengthMessage =
        "Reference number cannot exceed 100 characters.";

    private const int ReferenceNumberMaxLength = 100;

    [AggregateHandler]
    public static async Task<(ComplianceItemRecorded, Wolverine.Marten.Events)> Handle(
        RecordComplianceItem command,
        Project aggregate,
        IProjectService service,
        IClock clock,
        CancellationToken ct
    )
    {
        ArgumentNullException.ThrowIfNull(aggregate);
        service.ValidateAggregateUpdate(aggregate, command.OrganizationId);

        // The catalogue decides what belongs here. Recording a German permit against a Belgian
        // project would put a tick next to an obligation nobody has, which reads as compliance.
        if (
            !ComplianceCatalogue.Contains(
                aggregate.Assignment.WorkCountry,
                aggregate.EngagementType,
                command.Requirement
            )
        )
            throw new BusinessRuleException(NotInCatalogueMessage);

        if (command.DocumentId is not null && aggregate.DocumentById(command.DocumentId.Value) is null)
            throw new BusinessRuleException(UnknownDocumentMessage);

        var item = BuildItem(command);
        var user = await service.GetUserAsync(command.ModifiedBy, ct);

        var @event = new ComplianceItemRecorded(
            aggregate.Id.Value,
            aggregate.OrganizationId.Value,
            item,
            user,
            clock.UtcNow
        );

        return (@event, [@event]);
    }

    private static ComplianceItem BuildItem(RecordComplianceItem command)
    {
        var errors = new List<string>();

        var reference = command.ReferenceNumber?.Trim();

        if (reference is { Length: > ReferenceNumberMaxLength })
            errors.Add(ReferenceNumberMaxLengthMessage);

        if (
            command.Status is ComplianceStatus.Confirmed
            && ComplianceCatalogue.RequiresReferenceNumber(command.Requirement)
            && string.IsNullOrWhiteSpace(reference)
        )
            errors.Add(ReferenceNumberRequiredMessage);

        if (command.ValidTo is not null && command.ValidFrom is not null && command.ValidTo < command.ValidFrom)
            errors.Add(ValidToBeforeValidFromMessage);

        var (note, noteError) = ShortNote.TryCreate(command.Note ?? "", false);
        if (noteError is not null)
            errors.Add(noteError);

        if (errors.Count > 0)
            throw new ValidationException(errors);

        return new ComplianceItem(
            command.Requirement,
            command.Status,
            string.IsNullOrWhiteSpace(reference) ? null : reference,
            command.ValidFrom,
            command.ValidTo,
            command.DocumentId,
            string.IsNullOrWhiteSpace(note?.Value) ? null : note.Value
        );
    }
}
