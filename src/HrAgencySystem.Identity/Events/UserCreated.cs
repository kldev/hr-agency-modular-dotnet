using HrAgencySystem.Identity.Domain;
using HrAgencySystem.SharedKernel.Snapshots;

namespace HrAgencySystem.Identity.Events;

public sealed record UserCreated(
    Guid UserId,
    Guid OrganizationId,
    string Email,
    string FirstName,
    string LastName,
    OrganizationRole Role,
    string PasswordHash,
    UserSnapshot CreatedBy,
    DateTimeOffset CreatedAt);