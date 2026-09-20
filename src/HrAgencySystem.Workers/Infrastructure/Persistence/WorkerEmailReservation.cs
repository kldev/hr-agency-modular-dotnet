namespace HrAgencySystem.Workers.Infrastructure.Persistence;

public sealed record WorkerEmailReservation(
    Guid Id,
    Guid OrganizationId,
    Guid WorkerId,
    string Email
);
