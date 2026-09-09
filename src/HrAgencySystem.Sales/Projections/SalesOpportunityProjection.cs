using HrAgencySystem.Sales.Domain.Opportunity;
using HrAgencySystem.Sales.Events.Opportunity;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.Sales.Projections;

public sealed record SalesOpportunityProjection(
    // ReSharper disable once NotAccessedPositionalProperty.Global
    Guid Id,
    Guid OrganizationId,
    Guid CompanyId,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    CompanySnapshot Company,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    string Title,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    string Description,
    SalesOpportunityStage Stage,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    decimal ExpectedValue,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    CurrencyCode CurrencyCode,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    DateTimeOffset? ExpectedCloseDate,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    string LostReason,
    Guid SalesOwnerId,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    UserSnapshot SalesOwner,
    DateTimeOffset CreatedAt,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    UserSnapshot CreatedBy,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    UserSnapshot? ModifiedBy,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    DateTimeOffset? ModifiedAt
)
{
    public static SalesOpportunityProjection Create(
        SalesOpportunityCreated @event)
    {
        return new SalesOpportunityProjection(
            @event.OpportunityId,
            @event.OrganizationId,
            @event.Company.Id,
            @event.Company,
            @event.Title,
            @event.Description,
            @event.Stage,
            @event.ExpectedValue,
            @event.Currency,
            @event.ExpectedCloseDate,
            "",
            @event.Owner.Id,
            @event.Owner,
            @event.CreatedAt,
            @event.CreatedBy,
            null,
            null
        );
    }

    public SalesOpportunityProjection Apply(SalesOpportunityUpdated @event)
    {
        return this with
        {
            Title = @event.Title,
            Description = @event.Description,
            ExpectedValue = @event.ExpectedValue,
            ExpectedCloseDate = @event.ExpectedCloseDate
        };
    }

    public SalesOpportunityProjection Apply(SalesOpportunityStageChanged @event)
    {

        return this with
        {
            Stage = @event.Stage,
            LostReason = @event.LostReason,
            ModifiedAt = @event.ChangedAt,
            ModifiedBy = @event.ChangedBy
        };
    }

    public SalesOpportunityProjection Apply(SalesOpportunityOwnerChanged @event)
    {
        return this with
        {
            SalesOwner = @event.Owner,
            ModifiedAt = @event.ChangedAt,
            ModifiedBy = @event.ChangedBy
        };
    }
}