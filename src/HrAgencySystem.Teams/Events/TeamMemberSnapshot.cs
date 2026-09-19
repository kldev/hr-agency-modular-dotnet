using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.Teams.Domain;

namespace HrAgencySystem.Teams.Events;

/// <summary>
/// A member as it travels in an event: the full user snapshot next to the team role. The aggregate
/// keeps only the id and the role — names belong to the event, so they stay true to the moment and
/// the read model never has to join.
/// </summary>
public sealed record TeamMemberSnapshot(UserSnapshot User, TeamRole Role);
