using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Web.Common;

namespace HrAgencySystem.Identity.Events;

public sealed record UserUpdated(
    Guid UserId,
    Guid OrganizationId,
    UserSnapshot ModifiedBy,
    ContactPerson Contact,
    DateTimeOffset ModifiedAt
);
