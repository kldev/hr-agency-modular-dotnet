using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Workers.Events;

public sealed record WorkAuthorisationRemoved(
    Guid WorkerId,
    Guid OrganizationId,
    Guid AuthorisationId,
    UserSnapshot ModifiedBy,
    DateTimeOffset ModifiedAt
);
