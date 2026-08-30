using HrAgencySystem.Company.Application.Port;
using HrAgencySystem.Company.Domain;
using HrAgencySystem.Company.Domain.ValueObjects;
using HrAgencySystem.SharedKernel.Tenant;
using Marten;

namespace HrAgencySystem.Company.Infrastructure.Persistence;

public sealed class CompanyTaxIdReservationRepository(
    IDocumentSession session)
    : ICompanyTaxIdReservationRepository
{
    public async Task<bool> ExitsAsync(OrganizationId organizationId, TaxId taxId,
        CancellationToken cancellationToken = default)
    {
        return await session.Query<CompanyTaxIdReservation>().WithTaxId(organizationId, taxId)
            .AnyAsync(cancellationToken);
    }

    public Task ReserveAsync(
        OrganizationId organizationId,
        TaxId taxId,
        CompanyId companyId,
        CancellationToken cancellationToken = default)
    {
        var reservation = new CompanyTaxIdReservation
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId.Value,
            TaxId = taxId.Value,
            CompanyId = companyId.Value
        };

        session.Insert(reservation);

        return Task.CompletedTask;
    }
}