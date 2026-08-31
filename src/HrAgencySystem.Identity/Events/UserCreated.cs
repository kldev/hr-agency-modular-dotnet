using HrAgencySystem.Identity.Domain;

namespace HrAgencySystem.Identity.Events;

public sealed record UserCreated(
    Guid UserId,
    Guid OrganizationId,
    string Email,
    string FirstName,
    string LastName,
    OrganizationRole Role,
    string PasswordHash,
    DateTimeOffset CreatedAt);