using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Teams.Events;

public sealed record TeamRenamed(
    Guid TeamId,
    Guid OrganizationId,
    string Name,
    UserSnapshot ModifiedBy,
    DateTimeOffset ModifiedAt
);
