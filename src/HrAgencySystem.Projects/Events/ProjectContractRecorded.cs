using HrAgencySystem.Projects.Domain;
using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Projects.Events;

public sealed record ProjectContractRecorded(
    Guid ProjectId,
    Guid OrganizationId,
    ProjectContract Contract,
    UserSnapshot ModifiedBy,
    DateTimeOffset ModifiedAt
);
