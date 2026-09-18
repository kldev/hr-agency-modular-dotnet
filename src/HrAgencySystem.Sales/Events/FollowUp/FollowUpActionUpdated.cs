using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Sales.Events.FollowUp;

public sealed record FollowUpActionUpdated(
    Guid FollowUpActionId,
    Guid OpportunityId,
    Guid OrganizationId,
    string Content,
    DateTimeOffset FollowDateTime,
    DateTimeOffset ModifiedAt,
    UserSnapshot ModifiedBy
);
