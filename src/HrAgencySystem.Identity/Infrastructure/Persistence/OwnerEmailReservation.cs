namespace HrAgencySystem.Identity.Infrastructure.Persistence;

public sealed record OwnerEmailReservation(Guid Id, String Email, String PasswordHash);