using HrAgencySystem.EmailTemplates.Contracts.Teams;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.Teams.Domain;
using HrAgencySystem.Teams.Events;
using HrAgencySystem.Teams.Services;
using Wolverine;
using Wolverine.Marten;

namespace HrAgencySystem.Teams.Application.Members.Add;

public static class AddTeamMemberHandler
{
    public const string DuplicateMemberMessage = "This person is already on the team.";

    [AggregateHandler]
    public static async Task<(TeamMemberAdded, Wolverine.Marten.Events, OutgoingMessages)> Handle(
        AddTeamMember command,
        Team aggregate,
        ITeamsService service,
        IClock clock,
        CancellationToken ct
    )
    {
        ArgumentNullException.ThrowIfNull(aggregate);
        service.ValidateAggregateUpdate(aggregate, command.OrganizationId);

        if (aggregate.HasMember(command.UserId))
            throw new BusinessRuleException(DuplicateMemberMessage);

        var member = await service.GetOrganizationMemberAsync(
            OrganizationId.From(command.OrganizationId),
            command.UserId,
            ct
        );
        var addedBy = await service.GetUserAsync(command.ModifiedBy, ct);

        var @event = new TeamMemberAdded(
            aggregate.Id.Value,
            aggregate.OrganizationId.Value,
            new TeamMemberSnapshot(member, command.Role),
            addedBy,
            clock.UtcNow
        );

        var messages = new OutgoingMessages();

        // Putting yourself on a team is not worth an email.
        if (member.Id != addedBy.Id)
        {
            messages.Add(
                new SendTeamMemberAdded(
                    Guid.NewGuid(),
                    nameof(AddTeamMemberHandler),
                    aggregate.Id.Value,
                    aggregate.Name.Value,
                    member.Email,
                    member.Fullname,
                    command.Role.ToString(),
                    addedBy.Fullname
                )
            );
        }

        return (@event, [@event], messages);
    }
}
