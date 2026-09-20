using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.Workers.Application.Port;
using HrAgencySystem.Workers.Application.RegisterWorker;
using HrAgencySystem.Workers.Domain;
using HrAgencySystem.Workers.Events;
using HrAgencySystem.Workers.Services;
using Wolverine.Marten;

namespace HrAgencySystem.Workers.Application.UpdateWorker;

public static class UpdateWorkerHandler
{
    [AggregateHandler]
    public static async Task<(WorkerUpdated, Wolverine.Marten.Events)> Handle(
        UpdateWorker command,
        Worker aggregate,
        IWorkersService service,
        IWorkerIdentityDocumentReservationRepository documents,
        IWorkerEmailReservationRepository emails,
        IWorkersQueryRepository workers,
        IClock clock,
        CancellationToken ct
    )
    {
        ArgumentNullException.ThrowIfNull(aggregate);
        service.ValidateAggregateUpdate(aggregate, command.OrganizationId);

        var today = DateOnly.FromDateTime(clock.UtcNow.UtcDateTime);
        var data = WorkerDataFactory.Create(command, today);

        var documentChanged =
            aggregate.IdentityDocument.Number != data.IdentityDocument.Number
            || aggregate.IdentityDocument.IssuingCountry != data.IdentityDocument.IssuingCountry;

        // A document only gets corrected, replaced or renewed - rarely, and never into somebody
        // else's. The reservation moves with it so the register keeps its one file per person.
        if (documentChanged)
        {
            if (
                await documents.ExistsAsync(
                    aggregate.OrganizationId.Value,
                    data.IdentityDocument.IssuingCountry,
                    data.IdentityDocument.Number,
                    ct
                )
            )
                throw new BusinessRuleException(
                    IWorkerIdentityDocumentReservationRepository.AlreadyUsedMessage
                );

            await documents.ChangeDocumentAsync(
                aggregate.OrganizationId.Value,
                aggregate.Id.Value,
                data.IdentityDocument.IssuingCountry,
                data.IdentityDocument.Number,
                ct
            );
        }

        // Editing a file must not be a way round the rule that keeps one person to one file: the
        // same checks run, this person excepted.
        var emailChanged = aggregate.Email?.Value != data.Email?.Value;

        if (
            emailChanged
            && data.Email is not null
            && await emails.ExistsAsync(aggregate.OrganizationId.Value, data.Email.Value, ct)
        )
            throw new BusinessRuleException(IWorkerEmailReservationRepository.AlreadyUsedMessage);

        var duplicate = await workers.FindDuplicate(
            aggregate.OrganizationId,
            data.Email?.Value,
            data.FirstName.Value,
            data.LastName.Value,
            data.PhoneNumber.Value,
            aggregate.Id.Value,
            ct
        );

        if (duplicate is not null)
            throw new BusinessRuleException(RegisterWorkerHandler.AlreadyOnFileMessage);

        if (emailChanged)
            await emails.ChangeEmailAsync(
                aggregate.OrganizationId.Value,
                aggregate.Id.Value,
                data.Email?.Value,
                ct
            );

        var user = await service.GetUserAsync(command.ModifiedBy, ct);

        var @event = new WorkerUpdated(
            aggregate.Id.Value,
            aggregate.OrganizationId.Value,
            data.FirstName.Value,
            data.LastName.Value,
            data.DateOfBirth,
            data.Citizenship.Value,
            data.IdentityDocument,
            data.Email?.Value,
            data.PhoneNumber.Value,
            data.Address,
            data.Note.Value,
            user,
            clock.UtcNow
        );

        return (@event, [@event]);
    }
}
