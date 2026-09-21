using HrAgencySystem.Projects.Application.Positions.Update;
using HrAgencySystem.Projects.Domain;
using HrAgencySystem.Projects.Events;
using HrAgencySystem.Projects.Services;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Time;
using Wolverine.Marten;

namespace HrAgencySystem.Projects.Application.Positions.Archive;

/// <summary>
/// Closing a role. There is no deleting one: at twenty-five positions on a large client, most of
/// them are roles that have run their course, and the answer to that is a shorter picker - not a
/// hole in the history of everybody who ever worked on one.
/// <para>
/// Archiving a position somebody is still assigned to is allowed on purpose. A role ends while the
/// people on it work out their notice; refusing would force whoever runs the project to keep a
/// closed role open until the last day, which is exactly when nobody remembers to close it.
/// </para>
/// </summary>
public static class ArchivePositionHandler
{
    public const string AlreadyArchivedMessage = "This position is already archived.";

    [AggregateHandler]
    public static async Task<(ProjectPositionArchived, Wolverine.Marten.Events)> Handle(
        ArchivePosition command,
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

        if (position.IsArchived)
            throw new BusinessRuleException(AlreadyArchivedMessage);

        var modifiedBy = await service.GetUserAsync(command.ModifiedBy, ct);

        var @event = new ProjectPositionArchived(
            aggregate.Id.Value,
            aggregate.OrganizationId.Value,
            command.PositionId,
            modifiedBy,
            clock.UtcNow
        );

        return (@event, [@event]);
    }
}
