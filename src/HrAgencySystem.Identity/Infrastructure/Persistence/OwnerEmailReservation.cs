namespace HrAgencySystem.Identity.Infrastructure.Persistence;

public sealed record OwnerEmailReservation(Guid Id,Guid OwnerId, String Email, String PasswordHash);