using HrAgencySystem.Sales.Domain.Opportunity;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.Sales.Events.Opportunity;

public sealed record SalesOpportunityCreated(
    Guid OpportunityId,
    Guid OrganizationId,
    CompanySnapshot Company,
    string Title,
    string Description,
    SalesOpportunityStage Stage,
    decimal ExpectedValue,
    CurrencyCode  Currency,
    DateTimeOffset? ExpectedCloseDate, 
    UserSnapshot Owner,
    DateTimeOffset CreatedAt,
    UserSnapshot CreatedBy
    );