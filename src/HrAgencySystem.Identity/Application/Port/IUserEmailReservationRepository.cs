using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.Identity.Application.Port;

public interface IUserEmailReservationRepository
{
    Task<bool> ExistAsync(OrganizationId organizationId, Email email, CancellationToken ct);
    Task ReserveAsync(OrganizationId organizationId, Email email);
}