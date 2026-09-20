using HrAgencySystem.SharedKernel.Commands;
using JasperFx;

namespace HrAgencySystem.Workers.Application.UpdateAssignment;

public sealed record UpdateAssignment(
    [property: Identity] Guid AssignmentId,
    Guid OrganizationId,
    string Position,
    DateOnly StartsOn,
    DateOnly? EndsOn,
    Guid ModifiedBy
) : IUpdateCommand;
