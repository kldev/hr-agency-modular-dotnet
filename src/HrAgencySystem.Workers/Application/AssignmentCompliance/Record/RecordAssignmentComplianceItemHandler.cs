using HrAgencySystem.Compliance;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.SharedKernel.ValueObjects;
using HrAgencySystem.Workers.Events;
using HrAgencySystem.Workers.Services;
using Wolverine.Marten;

namespace HrAgencySystem.Workers.Application.AssignmentCompliance.Record;

/// <summary>
/// Records one of this posting's per person obligations - the A1, the Limosa declaration, the local
/// employment contract.
/// <para>
/// This is the register CLAUDE.md said did not exist. The same requirement can now be answered
/// separately for every person on a project and for every period they are there, which is how the
/// documents are actually issued; what used to be one tick on the project stating that everybody
/// was covered has no way of being recorded any more.
/// </para>
/// </summary>
public static class RecordAssignmentComplianceItemHandler
{
    public const string NotInCatalogueMessage =
        "This requirement does not apply to the assignment's country and engagement type.";

    public const string BelongsToTheProjectMessage =
        "This requirement is carried by the engagement as a whole and belongs to the project, not to one person.";

    public const string ReferenceNumberRequiredMessage =
        "Confirming this requirement needs the reference number it was filed under.";

    public const string ValidToBeforeValidFromMessage = "The validity cannot end before it begins.";

    public const string UnknownDocumentMessage =
        "The referenced document is not attached to this assignment.";

    public const string ReferenceNumberMaxLengthMessage =
        "Reference number cannot exceed 100 characters.";

    private const int ReferenceNumberMaxLength = 100;

    [AggregateHandler]
    public static async Task<(AssignmentComplianceItemRecorded, Wolverine.Marten.Events)> Handle(
        RecordAssignmentComplianceItem command,
        Domain.Assignment aggregate,
        IWorkersService service,
        IClock clock,
        CancellationToken ct
    )
    {
        ArgumentNullException.ThrowIfNull(aggregate);
        service.ValidateAggregateUpdate(aggregate, command.OrganizationId);

        // The mirror image of the guard on the project side: what one level refuses, the other
        // accepts, and the catalogue is the only thing that decides which is which.
        if (ComplianceCatalogue.ScopeOf(command.Requirement) is ComplianceScope.Project)
            throw new BusinessRuleException(BelongsToTheProjectMessage);

        if (
            !ComplianceCatalogue.Contains(
                aggregate.Project.WorkCountry,
                aggregate.EngagementType,
                ComplianceScope.Assignment,
                command.Requirement
            )
        )
            throw new BusinessRuleException(NotInCatalogueMessage);

        if (
            command.DocumentId is not null
            && aggregate.DocumentById(command.DocumentId.Value) is null
        )
            throw new BusinessRuleException(UnknownDocumentMessage);

        var item = BuildItem(command);
        var user = await service.GetUserAsync(command.ModifiedBy, ct);

        var @event = new AssignmentComplianceItemRecorded(
            aggregate.Id.Value,
            aggregate.OrganizationId.Value,
            aggregate.WorkerId.Value,
            item,
            user,
            clock.UtcNow
        );

        return (@event, [@event]);
    }

    private static ComplianceItem BuildItem(RecordAssignmentComplianceItem command)
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

        if (
            command.ValidTo is not null
            && command.ValidFrom is not null
            && command.ValidTo < command.ValidFrom
        )
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
