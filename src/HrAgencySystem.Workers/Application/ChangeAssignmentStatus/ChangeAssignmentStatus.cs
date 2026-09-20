using HrAgencySystem.SharedKernel.Commands;
using HrAgencySystem.Workers.Domain;
using JasperFx;

namespace HrAgencySystem.Workers.Application.ChangeAssignmentStatus;

/// <summary>
/// <paramref name="EndsOn"/> is how an assignment is ended, because ending one is recording when it
/// actually ended - which is rarely the day that was planned, and is the date the next posting's
/// period gets checked against.
/// </summary>
public sealed record ChangeAssignmentStatus(
    [property: Identity] Guid AssignmentId,
    Guid OrganizationId,
    AssignmentStatus Status,
    DateOnly? EndsOn,
    string? Reason,
    Guid ModifiedBy
) : IUpdateCommand;
