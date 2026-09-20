using HrAgencySystem.LegalEntities.Application.Create;
using HrAgencySystem.LegalEntities.Application.Port;
using HrAgencySystem.LegalEntities.Domain;
using HrAgencySystem.LegalEntities.Events;
using HrAgencySystem.LegalEntities.Services;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Time;
using Wolverine.Marten;

namespace HrAgencySystem.LegalEntities.Application.Update;

public static class UpdateLegalEntityHandler
{
    [AggregateHandler]
    public static async Task<(LegalEntityUpdated, Wolverine.Marten.Events)> Handle(
        UpdateLegalEntity command,
        LegalEntity aggregate,
        ILegalEntitiesService service,
        ILegalEntityTaxIdReservationRepository reservations,
        IClock clock,
        CancellationToken ct
    )
    {
        service.ValidateAggregateUpdate(aggregate, command.OrganizationId);

        var data = LegalEntityDataFactory.Create(command);

        if (data.TaxId.Value != aggregate.TaxId.Value)
        {
            if (await reservations.ExistsAsync(aggregate.OrganizationId, data.TaxId, ct))
                throw new BusinessRuleException(
                    ILegalEntityTaxIdReservationRepository.AlreadyUsedMessage
                );

            await reservations.ChangeTaxIdAsync(
                aggregate.OrganizationId,
                aggregate.Id,
                data.TaxId,
                ct
            );
        }

        var modifiedBy = await service.GetUserAsync(command.ModifiedBy, ct);

        var @event = new LegalEntityUpdated(
            command.LegalEntityId,
            aggregate.OrganizationId.Value,
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
            modifiedBy,
            clock.UtcNow
        );

        return (@event, [@event]);
    }
}
