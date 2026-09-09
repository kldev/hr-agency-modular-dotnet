using HrAgencySystem.Sales.Domain.Opportunity;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.Sales.Events.Opportunity;

public sealed record OpportunityUpdated( 
    Guid SalesOpportunityId,
    Guid OrganizationId,   
    OpportunityStage Stage,
    string Title,
    string Description,
    decimal PreviousExpectedValue,
    decimal ExpectedValue,
    bool IsHotLead,
    CurrencyCode  Currency,
    DateTimeOffset? ExpectedCloseDate,
    UserSnapshot ModifiedBy,
    DateTimeOffset ModifiedAt);