using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Sales.Events.FollowUp;

public sealed record FollowUpActionCreated(
    Guid FollowUpActionId,
    Guid OpportunityId,
    Guid OrganizationId,
    string Content,
    DateTimeOffset FollowDateTime,
    DateTimeOffset CreatedAt,
    UserSnapshot CreatedBy
);
