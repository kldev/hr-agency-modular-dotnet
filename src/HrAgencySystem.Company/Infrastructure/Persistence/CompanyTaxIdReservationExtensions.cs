using HrAgencySystem.Company.Domain.ValueObjects;
using HrAgencySystem.SharedKernel.Tenant;

namespace HrAgencySystem.Company.Infrastructure.Persistence;

public static class CompanyTaxIdReservationExtensions
{
    public static IQueryable<CompanyTaxIdReservation> WithTaxId(
        this IQueryable<CompanyTaxIdReservation> query, OrganizationId organizationId, TaxId taxId)
    {
        return query.Where(z => z.OrganizationId == organizationId.Value && z.TaxId == taxId.Value);
    }
}