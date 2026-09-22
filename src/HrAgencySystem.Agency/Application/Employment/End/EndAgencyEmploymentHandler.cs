using HrAgencySystem.Agency.Application.Employment.ChangeTerms;
using HrAgencySystem.Agency.Domain.Employment;
using HrAgencySystem.Agency.Events;
using HrAgencySystem.Agency.Services;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Time;
using Wolverine.Marten;

namespace HrAgencySystem.Agency.Application.Employment.End;

public static class EndAgencyEmploymentHandler
{
    public const string EndsBeforeStartMessage =
        "An engagement cannot end before the day it began.";

    [AggregateHandler(ConcurrencyStyle.Exclusive)]
    public static async Task<(AgencyEmploymentEnded, Wolverine.Marten.Events)> Handle(
        EndAgencyEmployment command,
        AgencyEmployment aggregate,
        IAgencyService service,
        IClock clock,
        CancellationToken ct
    )
    {
        ArgumentNullException.ThrowIfNull(aggregate);
        service.ValidateAggregateUpdate(aggregate, command.OrganizationId);

        if (aggregate.IsEnded)
            throw new BusinessRuleException(ChangeAgencyEmploymentTermsHandler.AlreadyEndedMessage);

        if (command.EndsOn < aggregate.StartsOn)
            throw new BusinessRuleException(EndsBeforeStartMessage);

        var modifiedBy = await service.GetUserAsync(command.ModifiedBy, ct);

        var @event = new AgencyEmploymentEnded(
            aggregate.OrganizationId.Value,
            aggregate.UserId,
            command.EndsOn,
            modifiedBy,
            clock.UtcNow
        );

        return (@event, [@event]);
    }
}
