using HrAgencySystem.Company.Application.Suggestion;
using HrAgencySystem.Company.Domain;
using HrAgencySystem.Company.Events;
using HrAgencySystem.Recruitment.Contracts.IntegrationEvents;
using HrAgencySystem.SharedKernel.Snapshots;


namespace HrAgencySystem.Company.Projections;

public sealed record CompanyProjection(
    Guid Id,
    Guid OrganizationId,
    string Name,
    string CountryCode,
    string TaxId,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    string RegistrationNumber,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    CompanyStatus Status,
    string Website,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    Guid CreatedId,
    UserSnapshot CreatedBy,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    DateTimeOffset CreatedAt,
    int JobsPostCount,
    int ActiveJobsCount,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    int ApplicantsCount)
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
            "",
            @event.CreatedBy.Id,
            @event.CreatedBy,
            @event.CreatedAt,
            0,
            0,
            0
        );
    }

    public CompanySuggestion ToSuggestion() => new(Id, Name, TaxId, CountryCode);

    public CompanyProjection Apply(CompanyJobPostCreated @event)
    {
        return this with
        {
            JobsPostCount = JobsPostCount + 1
        };
    }
    
    public CompanyProjection Apply(CompanyJobPostActiveChanged @event)
    {
        return this with
        {
            ActiveJobsCount = ActiveJobsCount + @event.ChangeBy
        };
    }
}