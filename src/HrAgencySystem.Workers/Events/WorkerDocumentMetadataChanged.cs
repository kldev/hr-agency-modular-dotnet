using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.Workers.Domain;

namespace HrAgencySystem.Workers.Events;

public sealed record WorkerDocumentMetadataChanged(
    Guid WorkerId,
    Guid OrganizationId,
    WorkerDocument Document,
    UserSnapshot ModifiedBy,
    DateTimeOffset ModifiedAt
);
