using HrAgencySystem.Projects.Domain;
using HrAgencySystem.Projects.Events;
using HrAgencySystem.Projects.Services;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Time;
using Wolverine.Marten;

namespace HrAgencySystem.Projects.Application.AssignTeam;

public static class AssignProjectTeamHandler
{
    public const string SameTeamMessage = "This team already runs the project.";

    [AggregateHandler]
    public static async Task<(ProjectTeamAssigned, Wolverine.Marten.Events)> Handle(
        AssignProjectTeam command,
        Project aggregate,
        IProjectService service,
        IClock clock,
        CancellationToken ct
    )
    {
        ArgumentNullException.ThrowIfNull(aggregate);
        service.ValidateAggregateUpdate(aggregate, command.OrganizationId);

        if (aggregate.TeamId == command.TeamId)
            throw new BusinessRuleException(SameTeamMessage);

        var team = await service.GetTeamAsync(
            OrganizationId.From(command.OrganizationId),
            command.TeamId,
            ct
        );
        var user = await service.GetUserAsync(command.ModifiedBy, ct);

        var @event = new ProjectTeamAssigned(
            aggregate.Id.Value,
            aggregate.OrganizationId.Value,
            team.TeamId,
            team.Name,
            user,
            clock.UtcNow
        );

        return (@event, [@event]);
    }
}
