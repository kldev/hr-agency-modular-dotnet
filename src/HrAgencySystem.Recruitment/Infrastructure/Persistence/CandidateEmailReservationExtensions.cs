using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.SharedKernel.ValueObjects;

namespace HrAgencySystem.Recruitment.Infrastructure.Persistence;

internal static class CandidateEmailReservationExtensions
{
    internal static IQueryable<CandidateEmailReservation> WithEmail(this IQueryable<CandidateEmailReservation> query,
        OrganizationId organizationId, Email email)
    {
        return query.Where(z => z.OrganizationId == organizationId.Value && z.Email == email.Value);
    }
}