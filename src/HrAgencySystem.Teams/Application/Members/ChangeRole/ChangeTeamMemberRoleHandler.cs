using HrAgencySystem.EmailTemplates.Contracts.Teams;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.Teams.Domain;
using HrAgencySystem.Teams.Events;
using HrAgencySystem.Teams.Services;
using Wolverine;
using Wolverine.Marten;

namespace HrAgencySystem.Teams.Application.Members.ChangeRole;

public static class ChangeTeamMemberRoleHandler
{
    public const string SameRoleMessage = "This person already holds that role on the team.";

    [AggregateHandler]
    public static async Task<(
        TeamMemberRoleChanged,
        Wolverine.Marten.Events,
        OutgoingMessages
    )> Handle(
        ChangeTeamMemberRole command,
        Team aggregate,
        ITeamsService service,
        IClock clock,
        CancellationToken ct
    )
    {
        ArgumentNullException.ThrowIfNull(aggregate);
        service.ValidateAggregateUpdate(aggregate, command.OrganizationId);

        var current =
            aggregate.FindMember(command.UserId)
            ?? throw new NotFoundException("Team member", command.UserId);

        if (current.Role == command.Role)
            throw new BusinessRuleException(SameRoleMessage);

        var member = await service.GetOrganizationMemberAsync(
            OrganizationId.From(command.OrganizationId),
            command.UserId,
            ct
        );
        var changedBy = await service.GetUserAsync(command.ModifiedBy, ct);

        var @event = new TeamMemberRoleChanged(
            aggregate.Id.Value,
            aggregate.OrganizationId.Value,
            new TeamMemberSnapshot(member, command.Role),
            current.Role,
            changedBy,
            clock.UtcNow
        );

        var messages = new OutgoingMessages();

        // Changing your own role is not worth an email.
        if (member.Id != changedBy.Id)
        {
            messages.Add(
                new SendTeamMemberRoleChanged(
                    Guid.NewGuid(),
                    nameof(ChangeTeamMemberRoleHandler),
                    aggregate.Id.Value,
                    aggregate.Name.Value,
                    member.Email,
                    member.Fullname,
                    command.Role.ToString(),
                    current.Role.ToString(),
                    changedBy.Fullname
                )
            );
        }

        return (@event, [@event], messages);
    }
}
