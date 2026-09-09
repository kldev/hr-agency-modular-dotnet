using HrAgencySystem.Sales.Domain.Opportunity;
using HrAgencySystem.Sales.Events.Opportunity;
using HrAgencySystem.Sales.Services;
using HrAgencySystem.SharedKernel.Time;
using Wolverine.Marten;

namespace HrAgencySystem.Sales.Application.Opportunities.ChangeResponsible;

public static class ChangeResponsiblePersonHandler
{
    [AggregateHandler]
    public static async Task<(ResponsiblePersonChanged, Wolverine.Marten.Events)> Handle(
        ChangeResponsiblePerson command,
        SalesOpportunity aggregate,
        ISalesService service,
        IClock clock,
        CancellationToken ct
    )
    {
        service.ValidateAggregateUpdate(aggregate, command.OrganizationId);

        var user = await service.GetUserAsync(command.ModifiedBy, ct);
        var responsible = await service.GetUserAsync(command.ResponsibleId, ct);
        
        var @event = new ResponsiblePersonChanged(
            aggregate.Id.Value,
            aggregate.ResponsiblePerson, 
            responsible, 
            user, 
            clock.UtcNow);

        return (@event, [@event]);
    }
    
}