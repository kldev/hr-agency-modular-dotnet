using HrAgencySystem.Company.Domain.ValueObjects;
using HrAgencySystem.Company.Events;
using HrAgencySystem.SharedKernel.Tenant;

namespace HrAgencySystem.Company.Domain;

public sealed class Company
{
    private Company()
    {
    }

    public CompanyId Id { get; private set; }

    public OrganizationId OrganizationId { get; private set; }

    public CompanyName Name { get; private set; } = null!;

    public CountryCode CountryCode { get; private set; } = null!;

    public TaxId? TaxId { get; private set; }

    public RegistrationNumber? RegistrationNumber { get; private set; }

    public CompanyStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public static Company Empty()
    {
        return new Company();
    }

    // private Company(
    //     CompanyId id,
    //     OrganizationId organizationId,
    //     CompanyName name,
    //     CountryCode countryCode,
    //     TaxId? taxId,
    //     RegistrationNumber? registrationNumber,
    //     DateTimeOffset now
    //     )
    // {
    //     Id = id;
    //     OrganizationId = organizationId;
    //     Name = name;
    //     CountryCode = countryCode;
    //     TaxId = taxId;
    //     RegistrationNumber = registrationNumber;
    //     Status = CompanyStatus.Active;
    //     CreatedAt = now;
    // }

    public void Apply(CompanyCreated @event)
    {
        Id = CompanyId.From(@event.CompanyId);
        OrganizationId = OrganizationId.From(@event.OrganizationId);
        Name = CompanyName.Create(@event.Name);
        CountryCode = CountryCode.Create(@event.CountryCode);

        TaxId = TaxId.Create(@event.TaxId);
        RegistrationNumber = RegistrationNumber.Create(@event.RegistrationNumber);

        Status = CompanyStatus.Active;
        CreatedAt = @event.CreatedAt;
    }
}