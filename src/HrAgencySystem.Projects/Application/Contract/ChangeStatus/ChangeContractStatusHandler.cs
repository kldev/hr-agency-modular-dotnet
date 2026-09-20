using HrAgencySystem.Projects.Domain;
using HrAgencySystem.Projects.Events;
using HrAgencySystem.Projects.Services;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Time;
using Wolverine.Marten;

namespace HrAgencySystem.Projects.Application.Contract.ChangeStatus;

public static class ChangeContractStatusHandler
{
    public const string NoContractMessage = "This project has no contract recorded yet.";
    public const string SameStatusMessage = "The contract is already in this status.";
    public const string SignedOnRequiredMessage = "A signed contract needs a signature date.";

    [AggregateHandler]
    public static async Task<(ProjectContractStatusChanged, Wolverine.Marten.Events)> Handle(
        ChangeContractStatus command,
        Project aggregate,
        IProjectService service,
        IClock clock,
        CancellationToken ct
    )
    {
        ArgumentNullException.ThrowIfNull(aggregate);
        service.ValidateAggregateUpdate(aggregate, command.OrganizationId);

        var contract =
            aggregate.Contract ?? throw new BusinessRuleException(NoContractMessage);

        if (contract.Status == command.Status)
            throw new BusinessRuleException(SameStatusMessage);

        var signedOn = command.SignedOn ?? contract.SignedOn;

        if (command.Status is ContractStatus.Signed && signedOn is null)
            throw new ValidationException(SignedOnRequiredMessage);

        var user = await service.GetUserAsync(command.ModifiedBy, ct);

        var @event = new ProjectContractStatusChanged(
            aggregate.Id.Value,
            aggregate.OrganizationId.Value,
            contract.Status,
            command.Status,
            signedOn,
            user,
            clock.UtcNow
        );

        return (@event, [@event]);
    }
}
