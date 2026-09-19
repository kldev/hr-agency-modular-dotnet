using HrAgencySystem.Identity.Domain;
using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Identity.Events;

public sealed record RoleChanged(
    Guid UserId,
    Guid OrganizationId,
    OrganizationRole PreviousRole,
    OrganizationRole Role,
    UserSnapshot ModifiedBy,
    DateTimeOffset ModifiedAt
);
