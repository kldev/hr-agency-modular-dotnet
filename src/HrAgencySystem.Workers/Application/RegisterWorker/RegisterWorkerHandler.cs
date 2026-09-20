using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.Workers.Application.Port;
using HrAgencySystem.Workers.Domain;
using HrAgencySystem.Workers.Events;
using HrAgencySystem.Workers.Services;
using Marten;

namespace HrAgencySystem.Workers.Application.RegisterWorker;

public static class RegisterWorkerHandler
{
    public const string AlreadyOnFileMessage =
        "This person is already on file. Plan a new assignment for them instead of opening a second file.";

    public static async Task<WorkerRegistered> Handle(
        RegisterWorker command,
        IWorkersService service,
        IWorkerIdentityDocumentReservationRepository documents,
        IWorkerEmailReservationRepository emails,
        IWorkersQueryRepository workers,
        IDocumentSession session,
        IClock clock,
        CancellationToken ct
    )
    {
        var organizationId = OrganizationId.From(command.OrganizationId);
        var today = DateOnly.FromDateTime(clock.UtcNow.UtcDateTime);
        var data = WorkerDataFactory.Create(command, today);

        await service.ValidateOrganization(command.OrganizationId, ct);

        // Three ways of noticing that this human being is already here, because one file per person
        // is what makes the register a register: a second one splits somebody's postings in half and
        // neither half can then be asked whether they hold a valid A1. Each check is reported
        // separately, because "already on file" without saying how we know is not actionable.
        //
        // The two reservations are checked for a readable answer; their unique indexes are what
        // settle two people filing the same passport in the same moment.
        if (
            await documents.ExistsAsync(
                organizationId.Value,
                data.IdentityDocument.IssuingCountry,
                data.IdentityDocument.Number,
                ct
            )
        )
            throw new BusinessRuleException(
                IWorkerIdentityDocumentReservationRepository.AlreadyUsedMessage
            );

        if (
            data.Email is not null
            && await emails.ExistsAsync(organizationId.Value, data.Email.Value, ct)
        )
            throw new BusinessRuleException(IWorkerEmailReservationRepository.AlreadyUsedMessage);

        var existing = await workers.FindDuplicate(
            organizationId,
            data.Email?.Value,
            data.FirstName.Value,
            data.LastName.Value,
            data.PhoneNumber.Value,
            null,
            ct
        );

        if (existing is not null)
            throw new BusinessRuleException(AlreadyOnFileMessage);

        var createdBy = await service.GetUserAsync(command.CreatedBy, ct);

        var workerId = WorkerId.New();

        // Everybody starts in recruitment, whichever door they came through. The pipeline is the
        // point: a file that appeared already employed would have skipped somebody's desk.
        var @event = new WorkerRegistered(
            workerId.Value,
            organizationId.Value,
            data.FirstName.Value,
            data.LastName.Value,
            data.DateOfBirth,
            data.Citizenship.Value,
            data.IdentityDocument,
            data.Email?.Value,
            data.PhoneNumber.Value,
            data.Address,
            data.Note.Value,
            command.SourceCandidateId,
            createdBy,
            clock.UtcNow
        );

        session.Events.StartStream<Worker>(workerId.Value, @event);

        await documents.ReserveAsync(
            organizationId.Value,
            workerId.Value,
            data.IdentityDocument.IssuingCountry,
            data.IdentityDocument.Number
        );

        if (data.Email is not null)
            await emails.ReserveAsync(organizationId.Value, workerId.Value, data.Email.Value);

        return @event;
    }
}
