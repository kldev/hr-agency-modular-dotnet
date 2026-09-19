using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.Teams.Domain;
using HrAgencySystem.Teams.Domain.ValueObjects;
using HrAgencySystem.Teams.Events;
using HrAgencySystem.Teams.Services;
using Marten;

namespace HrAgencySystem.Teams.Application.Create;

public static class CreateTeamHandler
{
    public const string EmptyMembersMessage = "A team needs at least one member.";
    public const string DuplicateMemberMessage = "The same person cannot be on the team twice.";

    public static async Task<TeamCreated> Handle(
        CreateTeam command,
        IDocumentSession session,
        ITeamsService service,
        IClock clock,
        CancellationToken ct
    )
    {
        var organizationId = OrganizationId.From(command.OrganizationId);
        var name = CreateName(command.Name);

        ValidateComposition(command);

        await service.ValidateOrganization(organizationId.Value, ct);

        var createdBy = await service.GetUserAsync(command.CreatedBy, ct);
        var members = await ResolveMembers(command, service, organizationId, ct);

        var teamId = TeamId.New();

        var @event = new TeamCreated(
            teamId.Value,
            organizationId.Value,
            name.Value,
            members,
            createdBy,
            clock.UtcNow
        );

        session.Events.StartStream<Team>(teamId.Value, @event);

        // No mail for the founding roster: filling in a creation form is not a handover.
        return @event;
    }

    private static void ValidateComposition(CreateTeam command)
    {
        if (command.Members.Count == 0)
            throw new BusinessRuleException(EmptyMembersMessage);

        var distinct = command.Members.Select(m => m.UserId).Distinct().Count();

        if (distinct != command.Members.Count)
            throw new BusinessRuleException(DuplicateMemberMessage);
    }

    private static async Task<IReadOnlyList<TeamMemberSnapshot>> ResolveMembers(
        CreateTeam command,
        ITeamsService service,
        OrganizationId organizationId,
        CancellationToken ct
    )
    {
        var members = new List<TeamMemberSnapshot>(command.Members.Count);

        foreach (var member in command.Members)
        {
            var user = await service.GetOrganizationMemberAsync(organizationId, member.UserId, ct);

            members.Add(new TeamMemberSnapshot(user, member.Role));
        }

        return members;
    }

    private static TeamName CreateName(string value)
    {
        var (name, error) = TeamName.TryCreate(value);

        return error != null ? throw new ValidationException(error) : name!;
    }
}
