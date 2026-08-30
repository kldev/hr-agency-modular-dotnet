using HrAgencySystem.Company.Domain;
using HrAgencySystem.Company.Domain.ValueObjects;
using HrAgencySystem.SharedKernel.Tenant;

namespace HrAgencySystem.Company.Application.Port;

public interface ICompanyTaxIdReservationRepository
{
    Task<bool> ExitsAsync(OrganizationId organizationId, TaxId taxId, CancellationToken cancellationToken = default);

    Task ReserveAsync(
        OrganizationId organizationId,
        TaxId taxId,
        CompanyId companyId,
        CancellationToken cancellationToken = default);
}