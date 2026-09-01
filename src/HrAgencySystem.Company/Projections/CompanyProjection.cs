using HrAgencySystem.Company.Domain;
using HrAgencySystem.Company.Events;


namespace HrAgencySystem.Company.Projections;

public sealed record CompanyProjection( 
    Guid Id, 
    Guid OrganizationId, 
    string Name, 
    string CountryCode, 
    string TaxId,
    string RegistrationNumber,
    CompanyStatus Status,
    DateTimeOffset CreatedAt)
{
    public static CompanyProjection Create(CompanyCreated @event)
    {
        return new CompanyProjection(
            @event.CompanyId,
            @event.OrganizationId,
            @event.Name,
            @event.CountryCode,
            @event.TaxId,
            @event.RegistrationNumber,
            CompanyStatus.Active,
            @event.CreatedAt
        );
    }
}