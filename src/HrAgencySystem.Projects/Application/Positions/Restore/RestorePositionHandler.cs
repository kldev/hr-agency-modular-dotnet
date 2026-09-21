using HrAgencySystem.Projects.Application.Positions.Open;
using HrAgencySystem.Projects.Application.Positions.Update;
using HrAgencySystem.Projects.Domain;
using HrAgencySystem.Projects.Events;
using HrAgencySystem.Projects.Services;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Time;
using Wolverine.Marten;

namespace HrAgencySystem.Projects.Application.Positions.Restore;

public static class RestorePositionHandler
{
    public const string NotArchivedMessage = "This position is not archived.";

    [AggregateHandler]
    public static async Task<(ProjectPositionRestored, Wolverine.Marten.Events)> Handle(
        RestorePosition command,
        Project aggregate,
        IProjectService service,
        IClock clock,
        CancellationToken ct
    )
    {
        ArgumentNullException.ThrowIfNull(aggregate);
        service.ValidateAggregateUpdate(aggregate, command.OrganizationId);

        var position =
            aggregate.PositionById(command.PositionId)
            ?? throw new BusinessRuleException(UpdatePositionHandler.UnknownPositionMessage);

        if (!position.IsArchived)
            throw new BusinessRuleException(NotArchivedMessage);

        // While it was away somebody may have opened a role under the same name, and two live
        // positions called the same thing are two roles nobody can tell apart.
        if (aggregate.HasPositionNamed(position.Name, position.PositionId))
            throw new BusinessRuleException(OpenPositionHandler.NameAlreadyUsedMessage);

        var modifiedBy = await service.GetUserAsync(command.ModifiedBy, ct);

        var @event = new ProjectPositionRestored(
            aggregate.Id.Value,
            aggregate.OrganizationId.Value,
            command.PositionId,
            modifiedBy,
            clock.UtcNow
        );

        return (@event, [@event]);
    }
}
