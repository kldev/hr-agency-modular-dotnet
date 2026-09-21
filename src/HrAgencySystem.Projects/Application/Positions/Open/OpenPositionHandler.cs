using HrAgencySystem.Projects.Domain;
using HrAgencySystem.Projects.Events;
using HrAgencySystem.Projects.Services;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Time;
using Wolverine.Marten;

namespace HrAgencySystem.Projects.Application.Positions.Open;

public static class OpenPositionHandler
{
    public const string NameAlreadyUsedMessage =
        "This project already has a position with that name. Names tell two roles apart, so they have to differ.";

    [AggregateHandler]
    public static async Task<(ProjectPositionOpened, Wolverine.Marten.Events)> Handle(
        OpenPosition command,
        Project aggregate,
        IProjectService service,
        IClock clock,
        CancellationToken ct
    )
    {
        ArgumentNullException.ThrowIfNull(aggregate);
        service.ValidateAggregateUpdate(aggregate, command.OrganizationId);

        var data = PositionDataFactory.Create(command);

        // Uniqueness inside one aggregate, settled by reading the aggregate - no reservation
        // document and no unique index. That is the whole payoff of a position living on the
        // project rather than in a register of its own.
        if (aggregate.HasPositionNamed(data.Name))
            throw new BusinessRuleException(NameAlreadyUsedMessage);

        var openedBy = await service.GetUserAsync(command.ModifiedBy, ct);

        var position = new ProjectPosition(
            Guid.CreateVersion7(),
            data.Name,
            data.ContractName,
            data.WorkDescription,
            data.Duties,
            data.RequiredQualifications,
            command.ContractType,
            data.Rate,
            data.WorkplaceAddress,
            data.WeeklyHours,
            data.WorkStartsAt,
            data.WorkSchedule,
            data.PayoutDay,
            data.ProbationPeriod,
            data.NoticePeriod,
            data.Allowances,
            data.PlannedHeadcount,
            command.DefaultEngagementType,
            false,
            openedBy,
            clock.UtcNow,
            null,
            null
        );

        var @event = new ProjectPositionOpened(
            aggregate.Id.Value,
            aggregate.OrganizationId.Value,
            position,
            openedBy,
            clock.UtcNow
        );

        return (@event, [@event]);
    }
}
