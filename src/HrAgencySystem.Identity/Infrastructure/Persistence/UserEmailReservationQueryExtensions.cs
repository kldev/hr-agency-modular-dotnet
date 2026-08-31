using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.Identity.Infrastructure.Persistence;

public static class UserEmailReservationQueryExtensions
{
    public static IQueryable<UserEmailReservation> WithEmail(
        this IQueryable<UserEmailReservation> query, OrganizationId organizationId, Email email)
    {
        return query.Where(z => z.OrganizationId == organizationId.Value && z.Email == email.Value);
    }
}