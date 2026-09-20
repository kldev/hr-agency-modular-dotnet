using HrAgencySystem.Compliance;
using HrAgencySystem.SharedKernel.Commands;

namespace HrAgencySystem.Workers.Application.PlanAssignment;

/// <summary>
/// <paramref name="EngagementType"/> is stated rather than taken from the project. One delivery can
/// post some people and employ others under local law, and which of the two applies decides whether
/// this person needs an A1 or a local contract - so it is said here, about this person, once.
/// </summary>
public sealed record PlanAssignment(
    Guid OrganizationId,
    Guid WorkerId,
    Guid ProjectId,
    EngagementType EngagementType,
    string Position,
    DateOnly StartsOn,
    DateOnly? EndsOn,
    Guid CreatedBy
) : ICreateCommand;
