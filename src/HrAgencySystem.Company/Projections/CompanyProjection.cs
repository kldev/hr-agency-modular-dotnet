using HrAgencySystem.Company.Application.Model;
using HrAgencySystem.Company.Domain;
using HrAgencySystem.Company.Events;
using HrAgencySystem.SharedKernel.Snapshots;


namespace HrAgencySystem.Company.Projections;

public sealed record CompanyProjection( 
    Guid Id, 
    Guid OrganizationId, 
    string Name, 
    string CountryCode, 
    string TaxId,
    string RegistrationNumber,
    CompanyStatus Status,
    Guid CreatedId,
    UserSnapshot CreatedBy,
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
            @event.CreatedBy.Id,
            @event.CreatedBy,
            @event.CreatedAt
        );
    }

    public CompanySuggestion ToSuggestion() => new (Id, Name, TaxId, CountryCode);
}