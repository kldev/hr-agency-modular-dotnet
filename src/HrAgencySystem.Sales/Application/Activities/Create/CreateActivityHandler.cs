using HrAgencySystem.Sales.Domain.Activity;
using HrAgencySystem.Sales.Events.Activity;
using HrAgencySystem.Sales.Events.Opportunity;
using HrAgencySystem.Sales.Services;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.SharedKernel.ValueObjects;
using Marten;

namespace HrAgencySystem.Sales.Application.Activities.Create;

public static class CreateActivityHandler
{
    public static async Task<ActivityCreated> Handle(
        CreateActivity command,
        ISalesService service,
        IDocumentSession session,
        IClock clock,
        CancellationToken ct
    )
    {
        await service.ValidateOrganization(command.OrganizationId, ct);

        var note = ShortNote.Create(command.Note, false);
        var user = await service.GetUserAsync(command.CreatedBy, ct);

        return await Log(
            session,
            service,
            SalesActivityId.New(),
            command.OrganizationId,
            command.SalesOpportunityId,
            command.ActivityType,
            note,
            user,
            clock.UtcNow,
            ct
        );
    }

    /// <summary>
    /// Writes an activity: its own stream, plus a copy on the opportunity's stream for the date of
    /// the last activity. Shared with the handler that logs a completed task, so both ways in
    /// leave the same trace.
    /// </summary>
    internal static async Task<ActivityCreated> Log(
        IDocumentSession session,
        ISalesService service,
        SalesActivityId activityId,
        Guid organizationId,
        Guid opportunityId,
        SalesActivityType type,
        ShortNote note,
        UserSnapshot user,
        DateTimeOffset at,
        CancellationToken ct
    )
    {
        var opportunity = await service.GetOpportunityAsync(organizationId, opportunityId, ct);
        var company = await service.GetCompanyAsync(opportunity.CompanyId, ct);

        var @event = new ActivityCreated(
            activityId.Value,
            organizationId,
            opportunity.OpportunityId,
            opportunity.Title,
            type,
            note.Value,
            at,
            user,
            company
        );

        session.Events.StartStream<SalesActivity>(activityId.Value, @event);
        session.Events.Append(
            opportunity.OpportunityId,
            new OpportunityActivityLogged(opportunity.OpportunityId, organizationId, activityId.Value, type, at)
        );

        return @event;
    }
}
