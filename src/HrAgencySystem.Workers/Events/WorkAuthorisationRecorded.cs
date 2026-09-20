using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.Workers.Domain;

namespace HrAgencySystem.Workers.Events;

public sealed record WorkAuthorisationRecorded(
    Guid WorkerId,
    Guid OrganizationId,
    WorkAuthorisation Authorisation,
    UserSnapshot ModifiedBy,
    DateTimeOffset ModifiedAt
);
