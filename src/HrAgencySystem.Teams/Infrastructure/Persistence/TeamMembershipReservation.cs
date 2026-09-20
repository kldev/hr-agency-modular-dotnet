namespace HrAgencySystem.Teams.Infrastructure.Persistence;

public sealed record TeamMembershipReservation(
    Guid Id,
    Guid OrganizationId,
    Guid UserId,
    Guid TeamId
);
