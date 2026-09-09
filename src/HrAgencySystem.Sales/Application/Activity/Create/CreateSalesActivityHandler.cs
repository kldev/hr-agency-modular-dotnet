using HrAgencySystem.Sales.Application.Queries;
using HrAgencySystem.Sales.Domain.Activity;
using HrAgencySystem.Sales.Events.Activity;
using HrAgencySystem.Sales.Services;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Port;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.SharedKernel.ValueObjects;
using Marten;

namespace HrAgencySystem.Sales.Application.Activity.Create;

public static class CreateSalesActivityHandler
{
    public static async Task<SalesActivityCreated> Handle(CreateSalesActivity command,
        ISalesService service,
        IDocumentSession session,
        IClock clock,
        CancellationToken ct)
    {
        var organizationId = OrganizationId.From(command.OrganizationId);
        await service.ValidateOrganization(command.OrganizationId, ct);
        
        var note = CreateValueObjects(command);

        var user = await service.GetUserAsync(command.CreatedBy, ct);
        var opportunity = await service.GetOpportunityAsync(command.OrganizationId,
            command.SalesOpportunityId, ct);
        var company = await service.GetCompanyAsync(opportunity.CompanyId, ct);

        var activityId = SalesActivityId.New();
        var @event = new SalesActivityCreated(
            activityId.Value,
            organizationId.Value, 
            opportunity.OpportunityId, 
            command.ActivityType, 
            note.Value,
            clock.UtcNow, user, company);

        session.Events.StartStream<SalesActivity>(activityId.Value, @event);
        return @event;
    }

    private static ShortNote CreateValueObjects(CreateSalesActivity command)
    {
        return ShortNote.Create(command.Note, false);
    }
}