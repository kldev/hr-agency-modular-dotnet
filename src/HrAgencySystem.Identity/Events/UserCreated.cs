using HrAgencySystem.Identity.Domain;
using HrAgencySystem.SharedKernel.Services;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Web.Common;
using HrAgencySystem.Teams.Contracts;

namespace HrAgencySystem.Identity.Events;

public sealed record UserCreated(
    Guid UserId,
    Guid OrganizationId,
    OrganizationRole Role,
    string PasswordHash,
    OrganizationInfo Organization,
    UserSnapshot CreatedBy,
    ContactPerson Contact,
    DateTimeOffset CreatedAt,
    TeamInfo? Team = null
);
