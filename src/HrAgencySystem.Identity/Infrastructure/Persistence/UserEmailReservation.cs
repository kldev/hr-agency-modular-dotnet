namespace HrAgencySystem.Identity.Infrastructure.Persistence;

public sealed record UserEmailReservation(Guid Id, Guid UserId, Guid OrganizationId, string Email, string PasswordHash);