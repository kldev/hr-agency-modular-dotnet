namespace HrAgencySystem.Projects.Events;

/// <summary>The other half of <see cref="ProjectPositionStaffed"/>: the seat is free again.</summary>
public sealed record ProjectPositionUnstaffed(
    Guid ProjectId,
    Guid OrganizationId,
    Guid PositionId,
    Guid AssignmentId
);
