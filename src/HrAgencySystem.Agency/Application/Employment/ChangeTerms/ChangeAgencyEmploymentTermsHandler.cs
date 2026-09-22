using HrAgencySystem.Agency.Application.Employment.Start;
using HrAgencySystem.Agency.Domain.Employment;
using HrAgencySystem.Agency.Events;
using HrAgencySystem.Agency.Services;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Time;
using Wolverine.Marten;

namespace HrAgencySystem.Agency.Application.Employment.ChangeTerms;

public static class ChangeAgencyEmploymentTermsHandler
{
    public const string UnknownEmploymentMessage =
        "This person has no employment record in this organization.";

    public const string AlreadyEndedMessage =
        "This engagement has ended. Start a new one rather than changing terms that no longer run.";

    public const string EffectiveBeforeStartMessage =
        "New terms cannot take effect before the engagement began.";

    [AggregateHandler(ConcurrencyStyle.Exclusive)]
    public static async Task<(AgencyEmploymentTermsChanged, Wolverine.Marten.Events)> Handle(
        ChangeAgencyEmploymentTerms command,
        AgencyEmployment aggregate,
        IAgencyService service,
        IClock clock,
        CancellationToken ct
    )
    {
        ArgumentNullException.ThrowIfNull(aggregate);
        service.ValidateAggregateUpdate(aggregate, command.OrganizationId);

        if (aggregate.IsEnded)
            throw new BusinessRuleException(AlreadyEndedMessage);

        if (command.EffectiveFrom < aggregate.StartsOn)
            throw new BusinessRuleException(EffectiveBeforeStartMessage);

        if (command.WeeklyHours is { } hours and (< 0 or > 168))
            throw new ValidationException(StartAgencyEmploymentHandler.WeeklyHoursRangeMessage);

        if (command.Rate is not null && !command.MayQuoteRate)
            throw new BusinessRuleException(StartAgencyEmploymentHandler.RateNotYoursMessage);

        var modifiedBy = await service.GetUserAsync(command.ModifiedBy, ct);

        var @event = new AgencyEmploymentTermsChanged(
            aggregate.OrganizationId.Value,
            aggregate.UserId,
            command.ContractType,
            command.EffectiveFrom,
            command.WeeklyHours,
            RateOf(command, aggregate),
            modifiedBy,
            clock.UtcNow
        );

        return (@event, [@event]);
    }

    /// <summary>
    /// The terms are replaced as a whole, and somebody who is not shown the rate cannot send it
    /// back. Without this, changing the hours of a person you manage would quietly erase their pay.
    /// </summary>
    private static SharedKernel.ValueObjects.WorkRate? RateOf(
        ChangeAgencyEmploymentTerms command,
        AgencyEmployment aggregate
    ) => command.MayQuoteRate ? RateInput.Validate(command.Rate) : aggregate.Rate;
}
