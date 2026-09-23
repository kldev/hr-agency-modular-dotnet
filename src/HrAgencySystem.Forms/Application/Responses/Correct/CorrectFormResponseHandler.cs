using HrAgencySystem.Forms.Application.Port;
using HrAgencySystem.Forms.Domain;
using HrAgencySystem.Forms.Domain.Responses;
using HrAgencySystem.Forms.Domain.Validation;
using HrAgencySystem.Forms.Events;
using HrAgencySystem.Forms.Services;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Time;
using Wolverine.Marten;

namespace HrAgencySystem.Forms.Application.Responses.Correct;

/// <summary>
/// A new revision of a submitted response, checked against the version it was submitted on - not
/// against whatever version is out today. Who may correct is decided at the door
/// (<c>FormsDesignPolicy</c>); this handler decides what a correction has to be.
/// </summary>
public static class CorrectFormResponseHandler
{
    public const int MaxReasonLength = 1000;

    public const string NotSubmittedMessage = "Only a submitted response is corrected. A draft is simply saved.";
    public const string ReasonRequiredMessage = "Say why the response is being corrected.";
    public const string ReasonTooLongMessage = "The reason cannot exceed 1000 characters.";

    [AggregateHandler]
    public static async Task<(FormResponseCorrected, Wolverine.Marten.Events)> Handle(
        CorrectFormResponse command,
        FormResponse aggregate,
        IFormsService service,
        IFormsRepository repository,
        IClock clock,
        CancellationToken ct
    )
    {
        service.ValidateAggregateUpdate(aggregate, command.OrganizationId, "Form response", command.ResponseId);

        if (aggregate.Status != FormResponseStatus.Submitted)
            throw new BusinessRuleException(NotSubmittedMessage);

        var reason = (command.Reason ?? "").Trim();

        if (reason.Length == 0)
            throw new ValidationException(ReasonRequiredMessage);

        if (reason.Length > MaxReasonLength)
            throw new ValidationException(ReasonTooLongMessage);

        var version = await ResponseAnswering.VersionOf(aggregate, repository, ct);
        var answers = ResponseAnswering.Check(version, command.Answers, ValidationMode.Submit);
        var user = await service.GetUserAsync(command.ModifiedBy, ct);
        var now = clock.UtcNow;

        // Only what the correction actually changes becomes the person's latest value. Fixing a typo
        // in last year's statement must not bring back the phone number they had last year.
        var changed = answers
            .Where(answer => aggregate.Answers.All(before =>
                before.FieldCode != answer.FieldCode || !before.Value.SameAs(answer.Value)))
            .ToList();

        await ResponseAnswering.UpdateProfile(aggregate, version, changed, now, repository, ct);

        var @event = new FormResponseCorrected(
            command.OrganizationId,
            command.ResponseId,
            aggregate.Revision + 1,
            answers,
            reason,
            user,
            now
        );

        return (@event, [@event]);
    }
}
