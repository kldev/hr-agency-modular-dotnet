namespace HrAgencySystem.Company.Application.Create;

internal interface ICompanyData
{
    string Name { get; }
    string  RegistrationNumber { get; }
    string WebSite { get; }
    string CountryCode { get; }
}