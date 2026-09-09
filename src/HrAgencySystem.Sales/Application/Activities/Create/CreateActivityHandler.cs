using HrAgencySystem.Sales.Domain.Activity;
using HrAgencySystem.Sales.Events.Activity;
using HrAgencySystem.Sales.Services;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.SharedKernel.ValueObjects;
using Marten;

namespace HrAgencySystem.Sales.Application.Activities.Create;

public static class CreateActivityHandler
{
    public static async Task<ActivityCreated> Handle(CreateActivity command,
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
        var @event = new ActivityCreated(
            activityId.Value,
            organizationId.Value, 
            opportunity.OpportunityId, 
            command.ActivityType, 
            note.Value,
            clock.UtcNow, user, company);

        session.Events.StartStream<SalesActivity>(activityId.Value, @event);
        return @event;
    }

    private static ShortNote CreateValueObjects(CreateActivity command)
    {
        return ShortNote.Create(command.Note, false);
    }
}