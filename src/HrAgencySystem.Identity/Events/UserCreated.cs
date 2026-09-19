using HrAgencySystem.Identity.Domain;
using HrAgencySystem.SharedKernel.Services;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Web.Common;

namespace HrAgencySystem.Identity.Events;

public sealed record UserCreated(
    Guid UserId,
    Guid OrganizationId,
    OrganizationRole Role,
    string PasswordHash,
    OrganizationInfo Organization,
    UserSnapshot CreatedBy,
    ContactPerson Contact,
    DateTimeOffset CreatedAt
);
