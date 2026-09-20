using HrAgencySystem.LegalEntities.Application.Port;
using HrAgencySystem.LegalEntities.Domain;
using HrAgencySystem.LegalEntities.Events;
using HrAgencySystem.LegalEntities.Services;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Time;
using Marten;

namespace HrAgencySystem.LegalEntities.Application.Create;

public static class CreateLegalEntityHandler
{
    public static async Task<LegalEntityCreated> Handle(
        CreateLegalEntity command,
        ILegalEntitiesService service,
        ILegalEntityTaxIdReservationRepository reservations,
        IDocumentSession session,
        IClock clock,
        CancellationToken ct
    )
    {
        var organizationId = OrganizationId.From(command.OrganizationId);
        var data = LegalEntityDataFactory.Create(command);

        await service.ValidateOrganization(command.OrganizationId, ct);

        // Checked for a friendly answer; the unique index is what actually settles a race.
        if (await reservations.ExistsAsync(organizationId, data.TaxId, ct))
            throw new BusinessRuleException(
                ILegalEntityTaxIdReservationRepository.AlreadyUsedMessage
            );

        var createdBy = await service.GetUserAsync(command.CreatedBy, ct);

        var legalEntityId = LegalEntityId.New();

        var @event = new LegalEntityCreated(
            legalEntityId.Value,
            organizationId.Value,
            data.Name.Value,
            data.LegalName.Value,
            data.TaxId.Value,
            data.VatNumber.Value,
            data.RegisteredAddress,
            data.Description.Value,
            data.President,
            data.BankAccounts,
            command.ActiveFrom,
            command.ActiveTo,
            createdBy,
            clock.UtcNow
        );

        session.Events.StartStream<LegalEntity>(legalEntityId.Value, @event);

        await reservations.ReserveAsync(organizationId, data.TaxId, legalEntityId);

        return @event;
    }
}
