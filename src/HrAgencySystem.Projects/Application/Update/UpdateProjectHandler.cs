using HrAgencySystem.Projects.Application.Create;
using HrAgencySystem.Projects.Domain;
using HrAgencySystem.Projects.Events;
using HrAgencySystem.Projects.Services;
using HrAgencySystem.SharedKernel.Time;
using Wolverine.Marten;

namespace HrAgencySystem.Projects.Application.Update;

public static class UpdateProjectHandler
{
    [AggregateHandler]
    public static async Task<(ProjectUpdated, Wolverine.Marten.Events)> Handle(
        UpdateProject command,
        Project aggregate,
        IProjectService service,
        IClock clock,
        CancellationToken ct
    )
    {
        ArgumentNullException.ThrowIfNull(aggregate);
        service.ValidateAggregateUpdate(aggregate, command.OrganizationId);

        // The client is fixed at creation, exactly as a job description's company is: moving a
        // project to another company would silently invalidate its contract and its declarations.
        var (name, description, assignment) = ProjectDataFactory.Create(command);
        var user = await service.GetUserAsync(command.ModifiedBy, ct);

        var @event = new ProjectUpdated(
            aggregate.Id.Value,
            aggregate.OrganizationId.Value,
            name.Value,
            description.Value,
            assignment,
            user,
            clock.UtcNow
        );

        return (@event, [@event]);
    }
}
