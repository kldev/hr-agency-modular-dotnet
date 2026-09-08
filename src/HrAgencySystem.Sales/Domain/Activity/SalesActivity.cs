using HrAgencySystem.Sales.Domain.Opportunity;
using HrAgencySystem.Sales.Events.Activity;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.Sales.Domain.Activity;

// ReSharper disable once ClassCannotBeInstantiated
public sealed class SalesActivity
{
    private SalesActivity(){}
    
    public SalesActivityId Id { get; private set; }
    public OrganizationId OrganizationId { get; private set; }
    public SalesOpportunityId SalesOpportunityId { get; private set; }
    public SalesActivityType ActivityType { get; private set; }
    public ShortNote Note { get; private set; } = null!;
    public DateTimeOffset CreatedAt { get; private set; }
    public UserSnapshot CreatedBy { get; private set; } = null!;
    public CompanySnapshot Company { get; private set; } = null!;

    public void Apply(SalesActivityCreated @event)
    {
        Id = SalesActivityId.From(@event.SalesActivityId);
        SalesOpportunityId = SalesOpportunityId.From(@event.SalesOpportunityId);
        OrganizationId = OrganizationId.From(@event.OrganizationId);
        ActivityType = @event.ActivityType;
        Note = ShortNote.Create(@event.Note, false);
        CreatedAt = @event.CreatedAt;
        CreatedBy = @event.CreatedBy;
    }
}