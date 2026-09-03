using HrAgencySystem.Company.Application.Commands;
using HrAgencySystem.Company.Application.Port;
using HrAgencySystem.Company.Domain;
using HrAgencySystem.Company.Domain.ValueObjects;
using HrAgencySystem.Company.Events;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.Port;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.Time;
using HrAgencySystem.SharedKernel.ValueObjects;
using Marten;

namespace HrAgencySystem.Company.Application.Handlers;

public static class CreateCompanyHandler
{
    public const string TaxIdAlreadyExistsMessage =
        "A company with the specified tax ID already exists in this organization.";
    
    public static async Task<CompanyCreated> Handle(
        CreateCompany command,
        IDocumentSession session,
        ICompanyTaxIdReservationRepository taxIdReservationRepository,
        IClock clock,
        IOrganizationChecker checker,
        IUserSnapshotRepository snapshotRepository,
        CancellationToken cancellationToken)
    {
        var organizationId = OrganizationId.From(command.OrganizationId);
        
        var (name, countryCode, taxId, registrationNumber) =
            CreateValueObjects(command);
        
        if (await taxIdReservationRepository.ExitsAsync(organizationId, taxId, cancellationToken))
            throw new BusinessRuleException(TaxIdAlreadyExistsMessage);

        if (!await checker.Exists(organizationId.Value, cancellationToken))
            throw new BusinessRuleException(OrganizationId.OrganizationCheckMessage);

        var createdBy = await snapshotRepository.GetUserAsync(command.CreatedBy, cancellationToken);
        if (createdBy == null)
            throw new BusinessRuleException(IUserSnapshotRepository.NotFoundMessage);
        
        var companyId = CompanyId.New();

        // The unique constraint protects against concurrent requests.
        await taxIdReservationRepository.ReserveAsync(
            organizationId,
            taxId,
            companyId,
            cancellationToken);

        var @event = new CompanyCreated(
            companyId.Value,
            organizationId.Value,
            name.Value,
            countryCode.Value,
            taxId.Value,
            registrationNumber.Value,
            createdBy,
            clock.UtcNow);

        session.Events.StartStream<Domain.Company>(companyId.Value, @event);

        return @event;
    }

    private static CompanyData CreateValueObjects(CreateCompany command)
    {
        var errors = new List<string>();

        var (name, nameError) = CompanyName.TryCreate(command.Name);
        if (nameError is not null)
            errors.Add(nameError);

        var (countryCode, countryError) =
            CountryCode.TryCreate(command.CountryCode);
        if (countryError is not null)
            errors.Add(countryError);

        var (taxId, taxIdError) = TaxId.TryCreate(command.TaxId);
        if (taxIdError is not null)
            errors.Add(taxIdError);

        var (registrationNumber, registrationNumberError) =
            RegistrationNumber.TryCreate(command.RegistrationNumber);
        if (registrationNumberError is not null)
            errors.Add(registrationNumberError);

        if (errors.Count > 0)
            throw new ValidationException(errors);

        return new CompanyData(
            name!,
            countryCode!,
            taxId!,
            registrationNumber!);
    }

    private sealed record CompanyData(
        CompanyName Name,
        CountryCode CountryCode,
        TaxId TaxId,
        RegistrationNumber RegistrationNumber);
}