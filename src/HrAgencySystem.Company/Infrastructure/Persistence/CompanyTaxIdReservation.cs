namespace HrAgencySystem.Company.Infrastructure.Persistence;

public sealed class CompanyTaxIdReservation
{
    public Guid Id { get; init; }

    public Guid OrganizationId { get; init; }

    public string TaxId { get; init; } = null!;

    public Guid CompanyId { get; set; }
}