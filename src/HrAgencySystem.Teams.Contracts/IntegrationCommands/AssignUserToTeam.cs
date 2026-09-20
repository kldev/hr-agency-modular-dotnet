namespace HrAgencySystem.Teams.Contracts.IntegrationCommands;

/// <summary>
/// A request to put somebody on a team, sent by the module that created the user. A command, not an
/// event — Teams is free to refuse it, which is why it does not live next to the integration events
/// describing things that already happened.
/// </summary>
public sealed record AssignUserToTeam(
    Guid TeamId,
    Guid OrganizationId,
    Guid UserId,
    TeamRole Role,
    Guid RequestedBy
);
