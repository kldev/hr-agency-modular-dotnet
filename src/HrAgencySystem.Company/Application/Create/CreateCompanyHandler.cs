using HrAgencySystem.Company.Application.Port;
using HrAgencySystem.Company.Domain;
using HrAgencySystem.Company.Domain.ValueObjects;
using HrAgencySystem.Company.Events;
using HrAgencySystem.Company.Services;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.SharedKernel.ValueObjects;
using Marten;

namespace HrAgencySystem.Company.Application.Create;

public static class CreateCompanyHandler
{
    public const string TaxIdAlreadyExistsMessage =
        "A company with the specified tax ID already exists in this organization.";
    
    public static async Task<CompanyCreated> Handle(
        CreateCompany command,
        IDocumentSession session,
        ICompanyTaxIdReservationRepository taxIdReservationRepository,
        IClock clock,
       ICompanyService service,
        CancellationToken ct)
    {
        var organizationId = OrganizationId.From(command.OrganizationId);
        
        var (name, registrationNumber, webSite, countryCode, taxId ) =
            CreateValueObjects(command);
        
        await ValidateTaxReservation(taxIdReservationRepository, ct, organizationId, taxId);

        await service.ValidateOrganization(organizationId.Value, ct);

        var createdBy = await service.GetUserAsync(command.CreatedBy, ct);

        var companyId = CompanyId.New();

        // The unique constraint protects against concurrent requests.
        await taxIdReservationRepository.ReserveAsync(
            organizationId,
            taxId,
            companyId,
            ct);

        var @event = new CompanyCreated(
            companyId.Value,
            organizationId.Value,
            name.Value,
            countryCode.Value,
            taxId.Value,
            registrationNumber.Value,
            command.Industry,
            webSite.Value,
            createdBy,
            clock.UtcNow);

        session.Events.StartStream<Domain.Company>(companyId.Value, @event);

        return @event;
    }
    
    private static async Task ValidateTaxReservation(ICompanyTaxIdReservationRepository taxIdReservationRepository,
        CancellationToken cancellationToken, OrganizationId organizationId, TaxId taxId)
    {
        if (await taxIdReservationRepository.ExitsAsync(organizationId, taxId, cancellationToken))
            throw new BusinessRuleException(TaxIdAlreadyExistsMessage);
    }

    private static CreateCompanyData CreateValueObjects(CreateCompany command)
    {
        var (data, errors) = CompanyDataFactory.CreateCompanyData(command, true);

        var (taxId, taxIdError) = TaxId.TryCreate(command.TaxId);
        if (taxIdError is not null)
            errors.Add(taxIdError);

        if (errors.Count > 0)
            throw new ValidationException(errors);

        return new CreateCompanyData(
            data.Name,
            data.RegistrationNumber,
            data.WebSite,
            data.CountryCode,
            taxId!
        );
    }

    private record CreateCompanyData(
        CompanyName Name,
        RegistrationNumber RegistrationNumber,
        WebSite WebSite,
        CountryCode CountryCode,
        TaxId TaxId
    );
}