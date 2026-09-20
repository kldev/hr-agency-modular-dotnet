using HrAgencySystem.Projects.Domain;
using HrAgencySystem.Projects.Events;
using HrAgencySystem.Projects.Services;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Time;
using Wolverine.Marten;

namespace HrAgencySystem.Projects.Application.Contacts.Remove;

public static class RemoveProjectContactHandler
{
    public const string RoleNotAssignedMessage = "Nobody holds this role on the project.";
    public const string ResponsibleRequiredWhileLiveMessage =
        "A live project cannot be left without a responsible contact.";

    [AggregateHandler]
    public static async Task<(ProjectContactRemoved, Wolverine.Marten.Events)> Handle(
        RemoveProjectContact command,
        Project aggregate,
        IProjectService service,
        IClock clock,
        CancellationToken ct
    )
    {
        ArgumentNullException.ThrowIfNull(aggregate);
        service.ValidateAggregateUpdate(aggregate, command.OrganizationId);

        var contact =
            aggregate.ContactInRole(command.Role)
            ?? throw new BusinessRuleException(RoleNotAssignedMessage);

        // The same condition that had to hold to go live has to keep holding while it is live;
        // otherwise a project could be running with nobody at the client to call.
        if (
            command.Role is ContactRole.Responsible
            && aggregate.Status is ProjectStatus.Active or ProjectStatus.Suspended
        )
            throw new BusinessRuleException(ResponsibleRequiredWhileLiveMessage);

        var user = await service.GetUserAsync(command.ModifiedBy, ct);

        var @event = new ProjectContactRemoved(
            aggregate.Id.Value,
            aggregate.OrganizationId.Value,
            command.Role,
            contact.Person,
            user,
            clock.UtcNow
        );

        return (@event, [@event]);
    }
}
