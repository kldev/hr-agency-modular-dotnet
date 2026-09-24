using HrAgencySystem.Forms.Application.Port;
using HrAgencySystem.Forms.Application.Responses.SaveDraft;
using HrAgencySystem.Forms.Domain;
using HrAgencySystem.Forms.Domain.Responses;
using HrAgencySystem.Forms.Domain.Validation;
using HrAgencySystem.Forms.Events;
using HrAgencySystem.Forms.Services;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Time;
using Wolverine.Marten;

namespace HrAgencySystem.Forms.Application.Responses.Submit;

/// <summary>
/// Makes the response a document: everything checked, required fields included, and the system
/// field answers taken as the person's latest values. An archived form does not stop this - the
/// response was started while the form was open and is bound to its own version.
/// </summary>
public static class SubmitFormResponseHandler
{
    [AggregateHandler]
    public static async Task<(FormResponseSubmitted, Wolverine.Marten.Events)> Handle(
        SubmitFormResponse command,
        FormResponse aggregate,
        IFormsService service,
        IFormsRepository repository,
        IClock clock,
        CancellationToken ct
    )
    {
        service.ValidateAggregateUpdate(aggregate, command.OrganizationId, "Form response", command.ResponseId);

        if (aggregate.Status != FormResponseStatus.Draft)
            throw new BusinessRuleException(SaveFormResponseDraftHandler.AlreadySubmittedMessage);

        var version = await ResponseAnswering.VersionOf(aggregate, repository, ct);
        var answers = ResponseAnswering.Check(version, command.Answers, ValidationMode.Submit);
        var user = await service.GetUserAsync(command.ModifiedBy, ct);
        var now = clock.UtcNow;

        await ResponseAnswering.UpdateProfile(aggregate, version, answers, now, repository, ct);

        var @event = new FormResponseSubmitted(command.OrganizationId, command.ResponseId, answers, user, now);

        return (@event, [@event]);
    }
}
