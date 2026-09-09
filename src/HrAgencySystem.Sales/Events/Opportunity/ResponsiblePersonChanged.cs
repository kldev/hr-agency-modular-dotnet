using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Sales.Events.Opportunity;

public sealed record ResponsiblePersonChanged(
    Guid OpportunityId, 
    UserSnapshot PreviousResponsible,
    UserSnapshot Responsible,
    UserSnapshot ChangedBy,
    DateTimeOffset ChangedAt);
