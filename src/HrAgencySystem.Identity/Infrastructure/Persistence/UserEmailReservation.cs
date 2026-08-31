namespace HrAgencySystem.Identity.Infrastructure.Persistence;

public sealed record UserEmailReservation(Guid Id, Guid OrganizationId, string Email, string PasswordHash);