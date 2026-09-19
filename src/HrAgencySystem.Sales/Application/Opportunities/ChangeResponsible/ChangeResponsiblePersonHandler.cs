using HrAgencySystem.EmailTemplates.Contracts.Sales;
using HrAgencySystem.Sales.Domain.Opportunity;
using HrAgencySystem.Sales.Events.Opportunity;
using HrAgencySystem.Sales.Services;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Time;
using Wolverine;
using Wolverine.Marten;

namespace HrAgencySystem.Sales.Application.Opportunities.ChangeResponsible;

public static class ChangeResponsiblePersonHandler
{
    public const string AlreadyAssignedError =
        "The specified person is already responsible for this opportunit";

    [AggregateHandler]
    public static async Task<(
        ResponsiblePersonChanged,
        Wolverine.Marten.Events,
        OutgoingMessages
    )> Handle(
        ChangeResponsiblePerson command,
        SalesOpportunity aggregate,
        ISalesService service,
        IClock clock,
        CancellationToken ct
    )
    {
        ArgumentNullException.ThrowIfNull(aggregate);
        service.ValidateAggregateUpdate(aggregate, command.OrganizationId);

        if (aggregate.ResponsiblePerson.Id == command.ResponsibleId)
            throw new BusinessRuleException(AlreadyAssignedError);

        var user = await service.GetUserAsync(command.ModifiedBy, ct);
        var responsible = await service.GetUserAsync(command.ResponsibleId, ct);

        var @event = new ResponsiblePersonChanged(
            aggregate.Id.Value,
            aggregate.ResponsiblePerson,
            responsible,
            user,
            clock.UtcNow
        );

        var messages = new OutgoingMessages();

        // Taking an opportunity over yourself is not worth an email.
        if (responsible.Id != user.Id)
        {
            messages.Add(
                new SendOpportunityResponsibleChanged(
                    Guid.NewGuid(),
                    nameof(ChangeResponsiblePersonHandler),
                    aggregate.Id.Value,
                    aggregate.Title.Value,
                    responsible.Email,
                    responsible.Fullname,
                    aggregate.ResponsiblePerson.Fullname,
                    user.Fullname
                )
            );
        }

        return (@event, [@event], messages);
    }
}
