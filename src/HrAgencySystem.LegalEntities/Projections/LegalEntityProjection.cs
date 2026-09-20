using HrAgencySystem.LegalEntities.Domain.ValueObjects;
using HrAgencySystem.LegalEntities.Events;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.LegalEntities.Projections;

public sealed record LegalEntityProjection(
    Guid Id,
    Guid OrganizationId,
    string Name,
    string LegalName,
    string TaxId,
    string? VatNumber,
    PostalAddress RegisteredAddress,
    string Description,
    President President,
    IReadOnlyList<LegalEntityBankAccount> BankAccounts,
    DateOnly ActiveFrom,
    DateOnly? ActiveTo,
    Guid CreatedById,
    UserSnapshot CreatedBy,
    DateTimeOffset CreatedAt,
    UserSnapshot? ModifiedBy = null,
    DateTimeOffset? ModifiedAt = null
) : IAudit
{
    public static LegalEntityProjection Create(LegalEntityCreated @event)
    {
        return new LegalEntityProjection(
            @event.LegalEntityId,
            @event.OrganizationId,
            @event.Name,
            @event.LegalName,
            @event.TaxId,
            @event.VatNumber,
            @event.RegisteredAddress,
            @event.Description,
            @event.President,
            @event.BankAccounts,
            @event.ActiveFrom,
            @event.ActiveTo,
            @event.CreatedBy.Id,
            @event.CreatedBy,
            @event.CreatedAt
        );
    }

    public LegalEntityProjection Apply(LegalEntityUpdated @event)
    {
        return this with
        {
            Name = @event.Name,
            LegalName = @event.LegalName,
            TaxId = @event.TaxId,
            VatNumber = @event.VatNumber,
            RegisteredAddress = @event.RegisteredAddress,
            Description = @event.Description,
            President = @event.President,
            BankAccounts = @event.BankAccounts,
            ActiveFrom = @event.ActiveFrom,
            ActiveTo = @event.ActiveTo,
            ModifiedBy = @event.ModifiedBy,
            ModifiedAt = @event.ModifiedAt,
        };
    }

    public LegalEntityProjection Apply(LegalEntityClosed @event)
    {
        return this with
        {
            ActiveTo = @event.ActiveTo,
            ModifiedBy = @event.ClosedBy,
            ModifiedAt = @event.ClosedAt,
        };
    }
}
