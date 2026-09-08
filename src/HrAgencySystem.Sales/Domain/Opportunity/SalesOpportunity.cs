using HrAgencySystem.Sales.Domain.Opportunity.ValueObjects;
using HrAgencySystem.Sales.Events.Opportunity;
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

    public void Apply(SalesOpportunityCreated @event)
    {
        Id = SalesOpportunityId.From(@event.SalesOpportunityId);
        OrganizationId = OrganizationId.From(@event.OrganizationId);
        Company = @event.Company;
        Title = OpportunityTitle.Create(@event.Title);
        Description = LongText.Create(@event.Description);
        Stage = SalesOpportunityStage.New;
        ExpectedValue = @event.ExpectedValue;
        CurrencyCode = @event.CurrencyCode;
        ExpectedCloseDate = @event.ExpectedCloseDate;
        LostReason = ShortNote.Create(@event.LostReason, false);
        SalesOwner = @event.SalesOwner;
        CreatedAt = @event.CreatedAt;
    }

    public void Apply(SalesOpportunityUpdated @event)
    {
        Title = OpportunityTitle.Create(@event.Title);
        Description = LongText.Create(@event.Description);
        Stage = SalesOpportunityStage.New;
        ExpectedValue = @event.ExpectedValue;
        ExpectedCloseDate = @event.ExpectedCloseDate;
    }

    public void Apply(SalesOpportunityStageChanged @event)
    {
        Stage = @event.Stage;
        if (@event.Stage == SalesOpportunityStage.Lost)
        {
            LostReason = ShortNote.Create(@event.LostReason);
        }
    }

    public void Apply(SalesOpportunityOwnerChanged @event)
    {
        SalesOwner = @event.Owner;
    }

}