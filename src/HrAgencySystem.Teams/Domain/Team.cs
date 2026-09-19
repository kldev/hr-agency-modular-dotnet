using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.Teams.Domain.ValueObjects;
using HrAgencySystem.Teams.Events;

namespace HrAgencySystem.Teams.Domain;

public sealed class Team : IOrganizationDomain
{
    private Team() { }

    public static Team Empty()
    {
        return new Team();
    }

    public TeamId Id { get; private set; }

    public OrganizationId OrganizationId { get; private set; }

    public TeamName Name { get; private set; } = null!;

    public IReadOnlyList<TeamMember> Members { get; private set; } = [];

    public DateTimeOffset CreatedAt { get; private set; }

    public Guid CreatedById { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public Guid? ModifiedById { get; private set; }

    public void Apply(TeamCreated @event)
    {
        Id = TeamId.From(@event.TeamId);
        OrganizationId = OrganizationId.From(@event.OrganizationId);
        Name = TeamName.Create(@event.Name);
        Members = [.. @event.Members.Select(m => new TeamMember(m.User.Id, m.Role))];
        CreatedAt = @event.CreatedAt;
        CreatedById = @event.CreatedBy.Id;
        UpdatedAt = @event.CreatedAt;
    }

    public void Apply(TeamRenamed @event)
    {
        Name = TeamName.Create(@event.Name);
        ApplyCommon(@event.ModifiedBy, @event.ModifiedAt);
    }

    public void Apply(TeamMemberAdded @event)
    {
        Members = [.. Members, new TeamMember(@event.Member.User.Id, @event.Member.Role)];
        ApplyCommon(@event.AddedBy, @event.OccurredAt);
    }

    public void Apply(TeamMemberRemoved @event)
    {
        Members = [.. Members.Where(m => m.UserId != @event.Member.User.Id)];
        ApplyCommon(@event.RemovedBy, @event.OccurredAt);
    }

    public void Apply(TeamMemberRoleChanged @event)
    {
        Members =
        [
            .. Members.Select(m =>
                m.UserId == @event.Member.User.Id ? m with { Role = @event.Member.Role } : m
            ),
        ];
        ApplyCommon(@event.ChangedBy, @event.OccurredAt);
    }

    public bool HasMember(Guid userId)
    {
        return Members.Any(m => m.UserId == userId);
    }

    public TeamMember? FindMember(Guid userId)
    {
        return Members.FirstOrDefault(m => m.UserId == userId);
    }

    private void ApplyCommon(UserSnapshot by, DateTimeOffset at)
    {
        UpdatedAt = at;
        ModifiedById = by.Id;
    }
}

public sealed record TeamMember(Guid UserId, TeamRole Role);
