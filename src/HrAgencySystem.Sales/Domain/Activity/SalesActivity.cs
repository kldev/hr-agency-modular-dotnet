using HrAgencySystem.Sales.Domain.Opportunity;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.Sales.Domain.Activity;

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
}