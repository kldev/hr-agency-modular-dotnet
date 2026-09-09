using HrAgencySystem.Sales.Domain.Opportunity;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.Sales.Events.Opportunity;

public sealed record OpportunityCreated(
    Guid OpportunityId,
    Guid OrganizationId,
    CompanySnapshot Company,
    string Title,
    string Description,
    OpportunityStage Stage,
    decimal ExpectedValue,
    CurrencyCode  Currency,
    bool IsHotLead,
    DateTimeOffset? ExpectedCloseDate, 
    UserSnapshot Responsible,
    DateTimeOffset CreatedAt,
    UserSnapshot CreatedBy
    );