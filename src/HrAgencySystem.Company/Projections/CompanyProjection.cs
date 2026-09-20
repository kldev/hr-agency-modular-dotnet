using HrAgencySystem.Company.Application.Suggestion;
using HrAgencySystem.Company.Domain;
using HrAgencySystem.Company.Events;
using HrAgencySystem.SharedKernel.Snapshots;
using HrAgencySystem.SharedKernel.Web.Common;

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
    // ReSharper disable once NotAccessedPositionalProperty.Global
    string Website,
    Industry Industry,
    Guid CreatedId,
    UserSnapshot CreatedBy,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    DateTimeOffset CreatedAt,
    Guid? ModifiedById,
    UserSnapshot? ModifiedBy,
    DateTimeOffset? ModifiedAt,
    int JobsPostCount,
    int ActiveJobsPostCount,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    int ApplicantsCount,
    ContactPerson? Contact,
    Guid? ContactPersonId,
    CompanyProfile Profile,
    // Flattened alongside Profile so "which clients can we sign a contract with" is a filter on a
    // column rather than a walk into a nested document.
    bool IsProfileComplete,
    DateTimeOffset? ProfileCompletedAt
)
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
            @event.Website,
            @event.Industry,
            @event.CreatedBy.Id,
            @event.CreatedBy,
            @event.CreatedAt,
            null,
            null,
            null,
            0,
            0,
            0,
            @event.Contact,
            @event.ContactPersonId,
            CompanyProfile.Empty,
            false,
            null
        );
    }

    public CompanyProjection Apply(CompanyProfileUpdated @event)
    {
        return this with
        {
            Profile = @event.Profile,
            IsProfileComplete = @event.Profile.IsComplete && !string.IsNullOrWhiteSpace(TaxId),
            ModifiedBy = @event.ModifiedBy,
            ModifiedById = @event.ModifiedBy.Id,
            ModifiedAt = @event.ModifiedAt,
        };
    }

    public CompanyProjection Apply(CompanyProfileCompleted @event)
    {
        return this with { ProfileCompletedAt = @event.CompletedAt };
    }

    public CompanyProjection Apply(CompanyJobPostCreated @event)
    {
        return this with { JobsPostCount = JobsPostCount + 1 };
    }

    public CompanyProjection Apply(CompanyJobPostActiveChanged @event)
    {
        return this with { ActiveJobsPostCount = ActiveJobsPostCount + @event.ChangeBy };
    }

    public CompanyProjection Apply(CompanyUpdated @event)
    {
        return this with
        {
            Name = @event.Name,
            Industry = @event.Industry,
            Website = @event.Website,
            RegistrationNumber = @event.RegistrationNumber,
            CountryCode = @event.CountryCode,
            ModifiedBy = @event.ModifiedBy,
            ModifiedById = @event.ModifiedBy.Id,
            ModifiedAt = @event.ModifiedAt,
        };
    }

    public CompanyProjection Apply(CompanyPrimaryContactUpdated @event)
    {
        return this with
        {
            Contact = @event.Contact,
            ContactPersonId = @event.ContactPersonId,
            ModifiedAt = @event.ModifiedAt,
        };
    }

    public CompanySuggestion ToSuggestion() => new(Id, Name, TaxId, CountryCode);
}
