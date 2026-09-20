using HrAgencySystem.Company.Domain.ValueObjects;
using HrAgencySystem.Company.Events;
using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.Company.Domain;

public sealed class Company
{
    private Company() { }

    public CompanyId Id { get; private set; }

    public OrganizationId OrganizationId { get; private set; }

    public CompanyName Name { get; private set; } = null!;

    public CountryCode CountryCode { get; private set; } = null!;

    public TaxId? TaxId { get; private set; }

    public RegistrationNumber? RegistrationNumber { get; private set; }

    public CompanyStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public Guid CreatedById { get; private set; }

    public Industry Industry { get; private set; }

    public WebSite WebSite { get; private set; } = null!;

    private CompanyProfile? _profile;

    /// <summary>
    /// The paperwork half, filled in separately from the lead. See <see cref="CompanyProfile"/>.
    /// <para>
    /// Backed by a nullable field because Marten rehydrates an aggregate without running field
    /// initialisers: a company whose stream carries no profile event would otherwise hand back null
    /// here, and every reader would have to know that.
    /// </para>
    /// </summary>
    public CompanyProfile Profile => _profile ?? CompanyProfile.Empty;

    /// <summary>
    /// Computed, never stored: a stored flag would keep claiming completeness after somebody clears
    /// the address. A project refuses to go live against a company for which this is false.
    /// </summary>
    public bool IsProfileComplete => Profile.IsComplete && TaxId is not null;

    public static Company Empty()
    {
        return new Company();
    }

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
        CreatedById = @event.CreatedBy.Id;
        Industry = @event.Industry;
        WebSite = WebSite.Create(@event.Website);
    }

    public void Apply(CompanyProfileUpdated @event)
    {
        _profile = @event.Profile;
    }

    public void Apply(CompanyProfileCompleted @event)
    {
        // Nothing to set: completeness is derived from the profile. The event exists so the stream
        // records when it happened and who did it.
    }

    public void Apply(CompanyUpdated @event)
    {
        Name = CompanyName.Create(@event.Name);
        CountryCode = CountryCode.Create(@event.CountryCode);
        RegistrationNumber = RegistrationNumber.Create(@event.RegistrationNumber);
        Industry = @event.Industry;
        WebSite = WebSite.Create(@event.Website);
    }
}
