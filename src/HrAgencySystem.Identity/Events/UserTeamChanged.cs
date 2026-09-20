using HrAgencySystem.Teams.Contracts;
using JasperFx;

namespace HrAgencySystem.Identity.Events;

/// <summary>
/// Where this user stands on teams now. Identity owns the event although Teams owns the fact: a
/// snapshot projection folds only events from its own stream, so the fact has to be restated here
/// before the user read model can see it.
///
/// A null <paramref name="Team"/> means the person is on no team.
/// </summary>
public sealed record UserTeamChanged(
    [property: Identity] Guid UserId,
    Guid OrganizationId,
    TeamInfo? Team,
    DateTimeOffset ChangedAt
);
