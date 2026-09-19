using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.Teams.Application.Suggestion;
using HrAgencySystem.Teams.Events;

namespace HrAgencySystem.Teams.Projections;

public sealed record TeamProjection(
    Guid Id,
    Guid OrganizationId,
    string Name,
    IReadOnlyList<TeamMemberSnapshot> Members,
    // Flattened alongside Members so "which teams is this person on" filters over an array of
    // primitives instead of a two-level document path.
    IReadOnlyList<Guid> MemberIds,
    Guid CreatedById,
    UserSnapshot CreatedBy,
    DateTimeOffset CreatedAt,
    Guid? ModifiedById,
    UserSnapshot? ModifiedBy,
    DateTimeOffset? ModifiedAt
)
{
    public static TeamProjection Create(TeamCreated @event)
    {
        return new TeamProjection(
            @event.TeamId,
            @event.OrganizationId,
            @event.Name,
            @event.Members,
            [.. @event.Members.Select(m => m.User.Id)],
            @event.CreatedBy.Id,
            @event.CreatedBy,
            @event.CreatedAt,
            null,
            null,
            null
        );
    }

    public TeamProjection Apply(TeamRenamed @event)
    {
        return this with
        {
            Name = @event.Name,
            ModifiedById = @event.ModifiedBy.Id,
            ModifiedBy = @event.ModifiedBy,
            ModifiedAt = @event.ModifiedAt,
        };
    }

    public TeamProjection Apply(TeamMemberAdded @event)
    {
        return WithMembers([.. Members, @event.Member], @event.AddedBy, @event.OccurredAt);
    }

    public TeamProjection Apply(TeamMemberRemoved @event)
    {
        return WithMembers(
            [.. Members.Where(m => m.User.Id != @event.Member.User.Id)],
            @event.RemovedBy,
            @event.OccurredAt
        );
    }

    public TeamProjection Apply(TeamMemberRoleChanged @event)
    {
        return WithMembers(
            [.. Members.Select(m => m.User.Id == @event.Member.User.Id ? @event.Member : m)],
            @event.ChangedBy,
            @event.OccurredAt
        );
    }

    public TeamSuggestion ToSuggestion()
    {
        return new TeamSuggestion(Id, Name, Members.Count);
    }

    private TeamProjection WithMembers(
        IReadOnlyList<TeamMemberSnapshot> members,
        UserSnapshot by,
        DateTimeOffset at
    )
    {
        return this with
        {
            Members = members,
            MemberIds = [.. members.Select(m => m.User.Id)],
            ModifiedById = by.Id,
            ModifiedBy = by,
            ModifiedAt = at,
        };
    }
}
