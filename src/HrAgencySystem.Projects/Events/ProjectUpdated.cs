using HrAgencySystem.Projects.Domain;
using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Projects.Events;

public sealed record ProjectUpdated(
    Guid ProjectId,
    Guid OrganizationId,
    string Name,
    string Description,
    Assignment Assignment,
    UserSnapshot ModifiedBy,
    DateTimeOffset ModifiedAt
);
