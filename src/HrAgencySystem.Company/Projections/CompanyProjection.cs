using HrAgencySystem.Company.Domain;
using HrAgencySystem.Company.Events;
using JasperFx.Events;


namespace HrAgencySystem.Company.Projections;

public sealed class CompanyProjection
{
    public Guid Id { get; set; }

    public Guid OrganizationId { get; set; }

    public string Name { get; set; } = null!;

    public string CountryCode { get; set; } = null!;

    public string? TaxId { get; set; }

    public string? RegistrationNumber { get; set; }

    public CompanyStatus? Status { get; set; } = null!;

    public DateTimeOffset CreatedAt { get; set; }

    public static CompanyProjection Create(IEvent<CompanyCreated> @event)
    {
        return new CompanyProjection()
        {
            Id = @event.Id,
            OrganizationId = @event.Data.OrganizationId,
            Name = @event.Data.Name,
            CountryCode = @event.Data.CountryCode,
            TaxId = @event.Data.TaxId,
            RegistrationNumber = @event.Data.RegistrationNumber,
            Status = CompanyStatus.Active,
            CreatedAt = @event.Data.CreatedAt
        };
    }
}