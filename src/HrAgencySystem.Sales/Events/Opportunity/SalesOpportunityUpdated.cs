using HrAgencySystem.Sales.Domain.Opportunity;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.Sales.Events.Opportunity;

public sealed record SalesOpportunityUpdated( 
    Guid SalesOpportunityId,
    Guid OrganizationId,   
    SalesOpportunityStage Stage,
    string Title,
    string Description,
    decimal PreviousExpectedValue,
    decimal ExpectedValue,
    CurrencyCode  CurrencyCode,
    DateTimeOffset? ExpectedCloseDate,
    UserSnapshot ModifiedBy);