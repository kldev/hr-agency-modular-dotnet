namespace HrAgencySystem.Workers.Contracts.IntegrationEvents;

/// <summary>
/// The seat is free again: the posting finished, was broken off, or the person never turned up.
/// Going live does not send this - an active assignment still occupies the role.
/// </summary>
public sealed record AssignmentPositionUnstaffed(
    Guid OrganizationId,
    Guid ProjectId,
    Guid PositionId,
    Guid AssignmentId
);
