using HrAgencySystem.LegalEntities.Projections;
using HrAgencySystem.SharedKernel.Tenant;

namespace HrAgencySystem.LegalEntities.Infrastructure.Query;

internal static class LegalEntityProjectionExtensions
{
    internal static IQueryable<LegalEntityProjection> WithOrganizationId(
        this IQueryable<LegalEntityProjection> query,
        OrganizationId organizationId
    )
    {
        return query.Where(z => z.OrganizationId == organizationId.Value);
    }

    internal static IQueryable<LegalEntityProjection> WithSearch(
        this IQueryable<LegalEntityProjection> query,
        string search
    )
    {
        if (string.IsNullOrWhiteSpace(search))
            return query;

        var normalized = search.Trim();

        return query.Where(z =>
            z.Name.Contains(normalized, StringComparison.OrdinalIgnoreCase)
            || z.LegalName.Contains(normalized, StringComparison.OrdinalIgnoreCase)
            || z.TaxId.Contains(normalized, StringComparison.OrdinalIgnoreCase)
        );
    }

    /// <summary>
    /// Trading on the given day. Worked out from the dates rather than a stored flag, so a company
    /// wound up yesterday drops out of the list today without anybody running anything.
    /// </summary>
    internal static IQueryable<LegalEntityProjection> WithActiveOnly(
        this IQueryable<LegalEntityProjection> query,
        bool activeOnly,
        DateOnly today
    )
    {
        return activeOnly
            ? query.Where(z => z.ActiveFrom <= today && (z.ActiveTo == null || z.ActiveTo >= today))
            : query;
    }

    internal static IQueryable<LegalEntityProjection> WithLegalEntityId(
        this IQueryable<LegalEntityProjection> query,
        Guid legalEntityId
    )
    {
        return query.Where(z => z.Id == legalEntityId);
    }
}
