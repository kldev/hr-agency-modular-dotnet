using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Sales.Events.Opportunity;

public sealed record SalesOpportunityOwnerChanged(
    Guid SalesOpportunityId, 
    UserSnapshot PreviousOwner,
    UserSnapshot Owner,
    UserSnapshot ChangedBy,
    DateTimeOffset ChangedAt);
