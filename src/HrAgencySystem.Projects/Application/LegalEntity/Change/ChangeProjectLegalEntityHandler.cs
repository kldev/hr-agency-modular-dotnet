using HrAgencySystem.Projects.Domain;
using HrAgencySystem.Projects.Events;
using HrAgencySystem.Projects.Services;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Time;
using Wolverine.Marten;

namespace HrAgencySystem.Projects.Application.LegalEntity.Change;

public static class ChangeProjectLegalEntityHandler
{
    public const string ProjectAlreadyStartedMessage =
        "The delivering company cannot be changed once the project has started. Copy the project instead.";

    public const string SameEntityMessage = "That is already the delivering company.";

    [AggregateHandler]
    public static async Task<(ProjectLegalEntityChanged, Wolverine.Marten.Events)> Handle(
        ChangeProjectLegalEntity command,
        Project aggregate,
        IProjectService service,
        IClock clock,
        CancellationToken ct
    )
    {
        service.ValidateAggregateUpdate(aggregate, command.OrganizationId);

        // Draft is the only state before a project starts, so this is the whole of the rule. After
        // that the contract, the notifications and the declarations all name this company - none of
        // which a field swap would put right.
        if (aggregate.Status is not ProjectStatus.Draft)
            throw new BusinessRuleException(ProjectAlreadyStartedMessage);

        if (aggregate.DeliveringEntity.LegalEntityId == command.LegalEntityId)
            throw new BusinessRuleException(SameEntityMessage);

        var entity = await service.GetLegalEntityAsync(
            aggregate.OrganizationId,
            command.LegalEntityId,
            aggregate.Placement.StartsOn,
            ct
        );

        var modifiedBy = await service.GetUserAsync(command.ModifiedBy, ct);

        var @event = new ProjectLegalEntityChanged(
            command.ProjectId,
            aggregate.OrganizationId.Value,
            DeliveringEntity.From(entity),
            modifiedBy,
            clock.UtcNow
        );

        return (@event, [@event]);
    }
}
