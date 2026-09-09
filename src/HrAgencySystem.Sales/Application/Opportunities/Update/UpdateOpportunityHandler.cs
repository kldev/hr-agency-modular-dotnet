using HrAgencySystem.Sales.Application.Opportunities.Create;
using HrAgencySystem.Sales.Domain.Opportunity;
using HrAgencySystem.Sales.Events.Opportunity;
using HrAgencySystem.Sales.Services;
using HrAgencySystem.SharedKernel.Time;
using Wolverine.Marten;

namespace HrAgencySystem.Sales.Application.Opportunities.Update;

public static class UpdateOpportunityHandler
{
    [AggregateHandler]
    public static async Task<(OpportunityUpdated, Wolverine.Marten.Events)> Handle(
        UpdateOpportunity command,
        SalesOpportunity aggregate,
        ISalesService service,
        IClock clock,
        CancellationToken ct)
    {
        
        ArgumentNullException.ThrowIfNull(aggregate);
        
        service.ValidateAggregateUpdate(aggregate, command.OrganizationId);
        var user = await service.GetUserAsync(command.ModifiedBy, ct);
        var (title, description) = OpportunityDataFactory.Create(command);
        
        var @event = new OpportunityUpdated(
            aggregate.Id.Value,
            aggregate.OrganizationId.Value,
            aggregate.Stage,
            title.Value,
            description.Value,
            aggregate.ExpectedValue,
            command.ExpectedValue,
            command.IsHotLead,
            command.Currency,
            command.ExpectedCloseDate,
            user,
            clock.UtcNow
            );
        
        return (@event, [@event]);
    }
    
}