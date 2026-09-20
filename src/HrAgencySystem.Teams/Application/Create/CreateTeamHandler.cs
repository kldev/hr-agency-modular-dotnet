using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.Teams.Application.Port;
using HrAgencySystem.Teams.Contracts.IntegrationEvents;
using HrAgencySystem.Teams.Domain;
using HrAgencySystem.Teams.Domain.ValueObjects;
using HrAgencySystem.Teams.Events;
using HrAgencySystem.Teams.Services;
using Marten;
using Wolverine;

namespace HrAgencySystem.Teams.Application.Create;

public static class CreateTeamHandler
{
    public const string EmptyMembersMessage = "A team needs at least one member.";
    public const string DuplicateMemberMessage = "The same person cannot be on the team twice.";

    public static async Task<(TeamCreated, OutgoingMessages)> Handle(
        CreateTeam command,
        IDocumentSession session,
        ITeamsService service,
        ITeamMembershipReservationRepository reservations,
        IClock clock,
        CancellationToken ct
    )
    {
        var organizationId = OrganizationId.From(command.OrganizationId);
        var name = CreateName(command.Name);

        ValidateComposition(command);

        await service.ValidateOrganization(organizationId.Value, ct);

        await ValidateNobodyIsOnAnotherTeam(command, reservations, organizationId, ct);

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

        var messages = new OutgoingMessages();

        foreach (var member in members)
        {
            await reservations.ReserveAsync(organizationId, member.User.Id, teamId.Value);

            messages.Add(
                TeamMembershipChanged.OnTeam(
                    member.User.Id,
                    organizationId.Value,
                    teamId.Value,
                    name.Value,
                    member.Role,
                    @event.CreatedAt
                )
            );
        }

        // No mail for the founding roster: filling in a creation form is not a handover. The
        // membership notices are not mail — they are how every module holding a copy finds out.
        return (@event, messages);
    }

    private static void ValidateComposition(CreateTeam command)
    {
        if (command.Members.Count == 0)
            throw new BusinessRuleException(EmptyMembersMessage);

        var distinct = command.Members.Select(m => m.UserId).Distinct().Count();

        if (distinct != command.Members.Count)
            throw new BusinessRuleException(DuplicateMemberMessage);
    }

    private static async Task ValidateNobodyIsOnAnotherTeam(
        CreateTeam command,
        ITeamMembershipReservationRepository reservations,
        OrganizationId organizationId,
        CancellationToken ct
    )
    {
        var assigned = await reservations.FindAssignedAsync(
            organizationId,
            [.. command.Members.Select(m => m.UserId)],
            ct
        );

        if (assigned.Count > 0)
            throw new BusinessRuleException(
                ITeamMembershipReservationRepository.AlreadyOnTeamMessage
            );
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
