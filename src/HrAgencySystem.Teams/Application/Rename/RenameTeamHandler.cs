using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.Teams.Contracts.IntegrationEvents;
using HrAgencySystem.Teams.Domain;
using HrAgencySystem.Teams.Domain.ValueObjects;
using HrAgencySystem.Teams.Events;
using HrAgencySystem.Teams.Services;
using Wolverine;
using Wolverine.Marten;

namespace HrAgencySystem.Teams.Application.Rename;

public static class RenameTeamHandler
{
    public const string SameNameMessage = "The team already goes by this name.";

    [AggregateHandler]
    public static async Task<(TeamRenamed, Wolverine.Marten.Events, OutgoingMessages)> Handle(
        RenameTeam command,
        Team aggregate,
        ITeamsService service,
        IClock clock,
        CancellationToken ct
    )
    {
        ArgumentNullException.ThrowIfNull(aggregate);
        service.ValidateAggregateUpdate(aggregate, command.OrganizationId);

        var (name, error) = TeamName.TryCreate(command.Name);

        if (error != null)
            throw new ValidationException(error);

        if (aggregate.Name.Value.Equals(name!.Value, StringComparison.Ordinal))
            throw new BusinessRuleException(SameNameMessage);

        var modifiedBy = await service.GetUserAsync(command.ModifiedBy, ct);

        var @event = new TeamRenamed(
            aggregate.Id.Value,
            aggregate.OrganizationId.Value,
            name.Value,
            modifiedBy,
            clock.UtcNow
        );

        // Everybody holding a copy of the old name has to hear about it, and the aggregate already
        // knows who that is — no roster lookup needed.
        var messages = new OutgoingMessages();

        foreach (var member in aggregate.Members)
        {
            messages.Add(
                TeamMembershipChanged.OnTeam(
                    member.UserId,
                    aggregate.OrganizationId.Value,
                    aggregate.Id.Value,
                    name.Value,
                    member.Role,
                    @event.ModifiedAt
                )
            );
        }

        return (@event, [@event], messages);
    }
}
