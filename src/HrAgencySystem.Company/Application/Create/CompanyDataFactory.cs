using HrAgencySystem.Company.Domain.ValueObjects;
using HrAgencySystem.SharedKernel.Exception;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.Company.Application.Create;

internal static class CompanyDataFactory
{
    internal static (CompanyData, List<string> errors) CreateCompanyData(ICompanyData data, bool skipValidation = false)
    {
        var errors = new List<string>();
        var (name, nameError) = CompanyName.TryCreate(data.Name);
        var (registrationNumber, registrationNumberError) =
            RegistrationNumber.TryCreate(data.RegistrationNumber);
        var (countryCode, countryError) =
            CountryCode.TryCreate(data.CountryCode);
        var (webSite, webSiteError) =
            WebSite.TryCreate(data.WebSite);

        if (nameError is not null)
            errors.Add(nameError);

        if (registrationNumberError is not null)
            errors.Add(registrationNumberError);
        
        if (countryError is not null)
            errors.Add(countryError);
        
        if (webSiteError is not null)
            errors.Add(webSiteError);

        if (!skipValidation && errors.Count > 0)
            throw new ValidationException(errors);

        return (new CompanyData(name!, registrationNumber!, webSite!, countryCode!), errors);
    }

    internal record CompanyData(
        CompanyName Name,
        RegistrationNumber RegistrationNumber,
        WebSite WebSite,
        CountryCode CountryCode);
}