using HrAgencySystem.SharedKernel.Commands;
using JasperFx;

namespace HrAgencySystem.Teams.Application.Members.Remove;

public sealed record RemoveTeamMember(
    [property: Identity] Guid TeamId,
    Guid OrganizationId,
    Guid UserId,
    Guid ModifiedBy
) : IUpdateCommand;
