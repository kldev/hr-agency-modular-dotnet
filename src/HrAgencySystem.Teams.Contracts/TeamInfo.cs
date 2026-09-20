namespace HrAgencySystem.Teams.Contracts;

/// <summary>
/// The team somebody belongs to, as other modules see it. Carries the name so a read model never has
/// to join, and the role so a user list can say what that person does on the team.
/// </summary>
public sealed record TeamInfo(Guid Id, string Name, TeamRole Role);
