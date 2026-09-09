using HrAgencySystem.Sales.Domain.Opportunity.ValueObjects;
using HrAgencySystem.Sales.Events.Opportunity;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.Sales.Domain.Opportunity;

public sealed class SalesOpportunity  : IOrganizationDomain
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
    public OpportunityStage Stage { get; private set; }
    public decimal ExpectedValue { get; private set; }
    public CurrencyCode CurrencyCode { get; private set; }
    public DateTimeOffset? ExpectedCloseDate { get; private set; }
    public ShortNote LostReason { get; private set; } = null!;
    public UserSnapshot ResponsiblePerson { get; private set; } = null!;
    public DateTimeOffset CreatedAt { get; private set; }
    
    public bool IsHotLead { get; private set; }

    public void Apply(OpportunityCreated @event)
    {
        Id = SalesOpportunityId.From(@event.OpportunityId);
        OrganizationId = OrganizationId.From(@event.OrganizationId);
        Company = @event.Company;
        Title = OpportunityTitle.Create(@event.Title);
        Description = LongText.Create(@event.Description);
        Stage = OpportunityStage.New;
        ExpectedValue = @event.ExpectedValue;
        CurrencyCode = @event.Currency;
        ExpectedCloseDate = @event.ExpectedCloseDate;
        ResponsiblePerson = @event.Responsible;
        CreatedAt = @event.CreatedAt;
        IsHotLead = @event.IsHotLead;
    }

    public void Apply(OpportunityUpdated @event)
    {
        Title = OpportunityTitle.Create(@event.Title);
        Description = LongText.Create(@event.Description);
        ExpectedValue = @event.ExpectedValue;
        ExpectedCloseDate = @event.ExpectedCloseDate;
        CurrencyCode = @event.Currency;
        IsHotLead = @event.IsHotLead;
    }

    public void Apply(StageChanged @event)
    {
        Stage = @event.Stage;
        if (@event.Stage == OpportunityStage.Lost)
        {
            LostReason = ShortNote.Create(@event.LostReason);
        }
    }

    public void Apply(ResponsiblePersonChanged @event)
    {
        ResponsiblePerson = @event.Responsible;
    }

}