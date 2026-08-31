using HrAgencySystem.Identity.Domain;

namespace HrAgencySystem.Identity.Events;

public sealed record PlatformOwnerCreated(
    Guid PlatformOwnerId,
    string Email,
    PlatformRole Role,
    string PasswordHash,
    DateTimeOffset CreatedAt);