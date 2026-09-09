using HrAgencySystem.Sales.Domain.Opportunity;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.Sales.Events.Opportunity;

public sealed record StageChanged(
    Guid OpportunityId,
    Guid OrganizationId,
    OpportunityStage PreviousStage,
    OpportunityStage Stage,
    UserSnapshot ChangedBy,
    DateTimeOffset ChangedAt,
    string LostReason,
    decimal ExpectedValue,
    CurrencyCode CurrencyCode
    );