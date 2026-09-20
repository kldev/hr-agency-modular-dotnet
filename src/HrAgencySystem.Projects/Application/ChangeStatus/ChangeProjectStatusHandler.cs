using HrAgencySystem.Projects.Domain;
using HrAgencySystem.Projects.Events;
using HrAgencySystem.Projects.Services;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.SharedKernel.ValueObjects;
using Wolverine.Marten;

namespace HrAgencySystem.Projects.Application.ChangeStatus;

public static class ChangeProjectStatusHandler
{
    public const string TransitionNotAllowedMessage = "This project status change is not allowed.";
    public const string ContractRequiredMessage =
        "An active project requires a signed contract.";
    public const string ResponsibleRequiredMessage =
        "An active project requires a responsible contact on the client side.";
    public const string CompanyProfileRequiredMessage =
        "The client company profile is incomplete: a legal name and a registered address are required.";

    [AggregateHandler]
    public static async Task<(ProjectStatusChanged, Wolverine.Marten.Events)> Handle(
        ChangeProjectStatus command,
        Project aggregate,
        IProjectService service,
        IClock clock,
        CancellationToken ct
    )
    {
        ArgumentNullException.ThrowIfNull(aggregate);
        service.ValidateAggregateUpdate(aggregate, command.OrganizationId);

        if (!ProjectStatusChangePolicy.Allow(aggregate.Status, command.Status))
            throw new BusinessRuleException(TransitionNotAllowedMessage);

        if (command.Status is ProjectStatus.Active)
            await EnsureReadyToGoLive(aggregate, service, ct);

        var user = await service.GetUserAsync(command.ModifiedBy, ct);
        var reason = ReadReason(command);

        var @event = new ProjectStatusChanged(
            aggregate.Id.Value,
            aggregate.OrganizationId.Value,
            aggregate.Status,
            command.Status,
            reason,
            user,
            clock.UtcNow
        );

        return (@event, [@event]);
    }

    /// <summary>
    /// The three things that have to be true before work starts. Each one is reported separately,
    /// because "the project is not ready" tells nobody what to go and do.
    /// </summary>
    private static async Task EnsureReadyToGoLive(
        Project aggregate,
        IProjectService service,
        CancellationToken ct
    )
    {
        if (aggregate.Contract is not { IsSigned: true })
            throw new BusinessRuleException(ContractRequiredMessage);

        if (aggregate.ContactInRole(ContactRole.Responsible) is null)
            throw new BusinessRuleException(ResponsibleRequiredMessage);

        // Read live rather than from the snapshot taken at creation: the profile is usually
        // completed after the project was drafted, and the question is whether it is complete now.
        var company = await service.GetCompanyAsync(
            OrganizationId.From(aggregate.OrganizationId.Value),
            aggregate.Company.Id,
            ct
        );

        if (!company.IsProfileComplete)
            throw new BusinessRuleException(CompanyProfileRequiredMessage);
    }

    private static string ReadReason(ChangeProjectStatus command)
    {
        if (string.IsNullOrWhiteSpace(command.Reason))
            return "";

        var (reason, error) = ShortNote.TryCreate(command.Reason);

        return error is not null ? throw new ValidationException(error) : reason!.Value;
    }
}
