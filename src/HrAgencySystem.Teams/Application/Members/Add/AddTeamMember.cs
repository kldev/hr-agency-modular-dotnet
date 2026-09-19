using HrAgencySystem.SharedKernel.Commands;
using HrAgencySystem.Teams.Domain;
using JasperFx;

namespace HrAgencySystem.Teams.Application.Members.Add;

public sealed record AddTeamMember(
    [property: Identity] Guid TeamId,
    Guid OrganizationId,
    Guid UserId,
    TeamRole Role,
    Guid ModifiedBy
) : IUpdateCommand;
