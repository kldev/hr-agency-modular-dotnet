using HrAgencySystem.SharedKernel.Commands;
using HrAgencySystem.Teams.Contracts;
using JasperFx;

namespace HrAgencySystem.Teams.Application.Members.ChangeRole;

public sealed record ChangeTeamMemberRole(
    [property: Identity] Guid TeamId,
    Guid OrganizationId,
    Guid UserId,
    TeamRole Role,
    Guid ModifiedBy
) : IUpdateCommand;
