using HrAgencySystem.Sales.Domain.Opportunity;
using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Sales.Events.Opportunity;

public sealed record SalesOpportunityStageChanged(
    Guid SalesOpportunityId,
    SalesOpportunityStage PreviousStage,
    SalesOpportunityStage Stage,
    UserSnapshot ChangedBy,
    DateTimeOffset ChangedAt,
    string LostReason
    );