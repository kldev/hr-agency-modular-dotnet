using HrAgencySystem.Sales.Domain.Opportunity;
using HrAgencySystem.Sales.Events.Opportunity;
using HrAgencySystem.Sales.Services;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.SharedKernel.ValueObjects;
using Wolverine.Marten;

namespace HrAgencySystem.Sales.Application.Opportunities.ChangeStage;

public  static class ChangeOpportunityStageHandler
{
    [AggregateHandler]
    public static async Task<(StageChanged, Wolverine.Marten.Events)>
        Handle(ChangeOpportunityStage command, 
            SalesOpportunity aggregate,
            ISalesService service,
            IClock clock,
            CancellationToken ct)
    {
        service.ValidateAggregateUpdate(aggregate, command.OrganizationId);
        
        if (aggregate.Stage == command.Stage)
            throw new BusinessRuleException("Opportunity is already at this stage");
        
        var user = await service.GetUserAsync(command.ModifiedBy, ct);
        var lostReason = GetLostReason(command);
        var @event = new StageChanged(aggregate.Id.Value,
            aggregate.OrganizationId.Value,
            aggregate.Stage,
            command.Stage,
            user,
            clock.UtcNow,
            lostReason,
            aggregate.ExpectedValue,
            aggregate.CurrencyCode
        );

        return (@event, [@event]);
    }

    private static string GetLostReason(ChangeOpportunityStage command)
    {
        if (command.Stage != OpportunityStage.Lost) return "";
        
        var (lostReason, error) = ShortNote.TryCreate(command.LostReason);
        return error != null ? throw new ValidationException(error) : lostReason!.Value;
    }
}