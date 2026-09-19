using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Identity.Events;

public sealed record PasswordChanged(
    Guid UserId,
    Guid OrganizationId,
    string PasswordHash,
    UserSnapshot ModifiedBy,
    DateTimeOffset ModifiedAt
);
