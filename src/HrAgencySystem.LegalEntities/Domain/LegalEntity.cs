using HrAgencySystem.LegalEntities.Domain.ValueObjects;
using HrAgencySystem.LegalEntities.Events;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.LegalEntities.Domain;

/// <summary>
/// One of the agency's own companies - the entity that signs the contract, issues the invoices and
/// carries the duties in the country where the work is done.
/// <para>
/// Not to be confused with <c>Company</c>, which is the client we invoice. An agency of any size
/// trades through several of these, and which one delivers a given project is a decision with legal
/// consequences rather than a label.
/// </para>
/// <para>
/// Replay only, like every other aggregate here: the rules live in the handlers, and Apply just puts
/// the state back.
/// </para>
/// </summary>
public sealed class LegalEntity : IOrganizationDomain
{
    private LegalEntity() { }

    public static LegalEntity Empty()
    {
        return new LegalEntity();
    }

    public LegalEntityId Id { get; private set; }

    public OrganizationId OrganizationId { get; private set; }

    public LegalEntityName Name { get; private set; } = null!;

    public LegalEntityName LegalName { get; private set; } = null!;

    public TaxId TaxId { get; private set; } = null!;

    public VatNumber VatNumber { get; private set; } = null!;

    public PostalAddress RegisteredAddress { get; private set; } = null!;

    public LongText Description { get; private set; } = null!;

    public President President { get; private set; } = null!;

    /// <summary>At most one per purpose and currency - see <see cref="LegalEntityBankAccount"/>.</summary>
    public IReadOnlyList<LegalEntityBankAccount> BankAccounts { get; private set; } = [];

    public DateOnly ActiveFrom { get; private set; }

    /// <summary>Null means it is still trading. A wound up entity keeps the day it stopped.</summary>
    public DateOnly? ActiveTo { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public Guid CreatedById { get; private set; }

    public DateTimeOffset? ModifiedAt { get; private set; }

    public Guid? ModifiedById { get; private set; }

    /// <summary>
    /// Worked out from the dates rather than stored. A kept flag would be right on the day it was
    /// written and wrong the morning after the entity's last day.
    /// </summary>
    public bool IsActiveOn(DateOnly date)
    {
        return date >= ActiveFrom && (ActiveTo is null || date <= ActiveTo);
    }

    /// <summary>The account an invoice in this currency should quote, if there is one.</summary>
    public LegalEntityBankAccount? AccountFor(BankAccountPurpose purpose, CurrencyCode currency)
    {
        return BankAccounts.FirstOrDefault(a => a.Purpose == purpose && a.Currency == currency);
    }

    public void Apply(LegalEntityCreated @event)
    {
        Id = LegalEntityId.From(@event.LegalEntityId);
        OrganizationId = OrganizationId.From(@event.OrganizationId);

        Name = LegalEntityName.Create(@event.Name);
        LegalName = LegalEntityName.Create(@event.LegalName);
        TaxId = TaxId.Create(@event.TaxId);
        VatNumber = VatNumber.Create(@event.VatNumber);
        RegisteredAddress = @event.RegisteredAddress;
        Description = LongText.Create(@event.Description, isRequired: false);
        President = @event.President;
        BankAccounts = @event.BankAccounts;

        ActiveFrom = @event.ActiveFrom;
        ActiveTo = @event.ActiveTo;

        CreatedAt = @event.CreatedAt;
        CreatedById = @event.CreatedBy.Id;
    }

    public void Apply(LegalEntityUpdated @event)
    {
        Name = LegalEntityName.Create(@event.Name);
        LegalName = LegalEntityName.Create(@event.LegalName);
        TaxId = TaxId.Create(@event.TaxId);
        VatNumber = VatNumber.Create(@event.VatNumber);
        RegisteredAddress = @event.RegisteredAddress;
        Description = LongText.Create(@event.Description, isRequired: false);
        President = @event.President;
        BankAccounts = @event.BankAccounts;

        ActiveFrom = @event.ActiveFrom;
        ActiveTo = @event.ActiveTo;

        ModifiedAt = @event.ModifiedAt;
        ModifiedById = @event.ModifiedBy.Id;
    }

    public void Apply(LegalEntityClosed @event)
    {
        ActiveTo = @event.ActiveTo;

        ModifiedAt = @event.ClosedAt;
        ModifiedById = @event.ClosedBy.Id;
    }
}
