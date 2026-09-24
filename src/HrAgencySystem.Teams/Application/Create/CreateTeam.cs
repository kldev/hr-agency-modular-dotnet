using HrAgencySystem.SharedKernel.Commands;
using HrAgencySystem.Teams.Contracts;

namespace HrAgencySystem.Teams.Application.Create;

public sealed record CreateTeam(
    Guid OrganizationId,
    string Name,
    IReadOnlyList<CreateTeamMember> Members,
    Guid CreatedBy
) : ICreateCommand;

public sealed record CreateTeamMember(Guid UserId, TeamRole Role);
