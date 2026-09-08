using HrAgencySystem.Sales.Domain.Opportunity.ValueObjects;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.Sales.Domain.Opportunity;

public sealed class SalesOpportunity
{
    private SalesOpportunity()
    {
        
    }

    public static SalesOpportunity Empty()
    {
        return new SalesOpportunity();
    }
    
    public SalesOpportunityId Id { get; private set; }
    public OrganizationId OrganizationId { get; private set; }
    public CompanySnapshot Company { get; private set; } = null!;
    public OpportunityTitle Title { get; private set; } = null!;
    public LongText Description { get; private set; } = null!;
    public SalesOpportunityStage Stage { get; private set; }
    public decimal ExpectedValue { get; private set; }
    public CurrencyCode CurrencyCode { get; private set; } 
    public DateTimeOffset? ExpectedCloseDate { get; private set; }
    public ShortNote LostReason { get; private set; } = null!;
    public UserSnapshot SalesOwner { get; private set; } = null!;
    public DateTimeOffset CreatedAt { get; private set; }
    
    
}