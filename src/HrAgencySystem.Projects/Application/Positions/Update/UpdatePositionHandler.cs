using HrAgencySystem.Projects.Application.Positions.Open;
using HrAgencySystem.Projects.Domain;
using HrAgencySystem.Projects.Events;
using HrAgencySystem.Projects.Services;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Time;
using Wolverine.Marten;

namespace HrAgencySystem.Projects.Application.Positions.Update;

public static class UpdatePositionHandler
{
    public const string UnknownPositionMessage = "This project has no such position.";

    [AggregateHandler]
    public static async Task<(ProjectPositionUpdated, Wolverine.Marten.Events)> Handle(
        UpdatePosition command,
        Project aggregate,
        IProjectService service,
        IClock clock,
        CancellationToken ct
    )
    {
        ArgumentNullException.ThrowIfNull(aggregate);
        service.ValidateAggregateUpdate(aggregate, command.OrganizationId);

        var existing =
            aggregate.PositionById(command.PositionId)
            ?? throw new BusinessRuleException(UnknownPositionMessage);

        var data = PositionDataFactory.Create(command);

        if (aggregate.HasPositionNamed(data.Name, command.PositionId))
            throw new BusinessRuleException(OpenPositionHandler.NameAlreadyUsedMessage);

        var modifiedBy = await service.GetUserAsync(command.ModifiedBy, ct);

        var position = existing with
        {
            Name = data.Name,
            ContractName = data.ContractName,
            WorkDescription = data.WorkDescription,
            Duties = data.Duties,
            RequiredQualifications = data.RequiredQualifications,
            ContractType = command.ContractType,
            ProposedRate = data.Rate,
            WorkplaceAddress = data.WorkplaceAddress,
            WeeklyHours = data.WeeklyHours,
            WorkStartsAt = data.WorkStartsAt,
            WorkSchedule = data.WorkSchedule,
            PayoutDay = data.PayoutDay,
            ProbationPeriod = data.ProbationPeriod,
            NoticePeriod = data.NoticePeriod,
            Allowances = data.Allowances,
            PlannedHeadcount = data.PlannedHeadcount,
            DefaultEngagementType = command.DefaultEngagementType,
            ModifiedBy = modifiedBy,
            ModifiedAt = clock.UtcNow,
        };

        // Said on the event rather than worked out by whoever reads it: a rename is the one change
        // that has to travel outside this module, because assignments froze this name when they
        // were planned.
        var nameChanged = !string.Equals(existing.Name, position.Name, StringComparison.Ordinal);

        var @event = new ProjectPositionUpdated(
            aggregate.Id.Value,
            aggregate.OrganizationId.Value,
            position,
            nameChanged,
            modifiedBy,
            clock.UtcNow
        );

        return (@event, [@event]);
    }
}
