using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Sales.Events.Opportunity;

public sealed record SalesOpportunityUpdated( 
    Guid SalesOpportunityId,
    Guid OrganizationId,   
    string Title,
    string Description,
    decimal ExpectedValue,
    DateTimeOffset? ExpectedCloseDate,
    UserSnapshot ModifiedBy);