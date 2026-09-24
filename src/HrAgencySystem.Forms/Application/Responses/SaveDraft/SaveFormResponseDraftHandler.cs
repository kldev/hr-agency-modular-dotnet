using HrAgencySystem.Forms.Application.Port;
using HrAgencySystem.Forms.Domain;
using HrAgencySystem.Forms.Domain.Responses;
using HrAgencySystem.Forms.Domain.Validation;
using HrAgencySystem.Forms.Events;
using HrAgencySystem.Forms.Services;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Time;
using Wolverine.Marten;

namespace HrAgencySystem.Forms.Application.Responses.SaveDraft;

/// <summary>
/// Checks what is filled in, not what is missing: a required field left empty is fine in a draft,
/// a date outside its range is not.
/// </summary>
public static class SaveFormResponseDraftHandler
{
    public const string AlreadySubmittedMessage =
        "This response is already submitted. It can only be corrected now.";

    [AggregateHandler]
    public static async Task<(FormResponseDraftSaved, Wolverine.Marten.Events)> Handle(
        SaveFormResponseDraft command,
        FormResponse aggregate,
        IFormsService service,
        IFormsRepository repository,
        IClock clock,
        CancellationToken ct
    )
    {
        service.ValidateAggregateUpdate(aggregate, command.OrganizationId, "Form response", command.ResponseId);

        if (aggregate.Status != FormResponseStatus.Draft)
            throw new BusinessRuleException(AlreadySubmittedMessage);

        var version = await ResponseAnswering.VersionOf(aggregate, repository, ct);
        var answers = ResponseAnswering.Check(version, command.Answers, ValidationMode.Draft);
        var user = await service.GetUserAsync(command.ModifiedBy, ct);

        var @event = new FormResponseDraftSaved(
            command.OrganizationId,
            command.ResponseId,
            answers,
            user,
            clock.UtcNow
        );

        return (@event, [@event]);
    }
}
