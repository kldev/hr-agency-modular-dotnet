using HrAgencySystem.Projects.Application.Port;
using HrAgencySystem.Projects.Projections;
using HrAgencySystem.SharedKernel.Tenant;

namespace HrAgencySystem.Projects.Infrastructure.Query;

internal static class ProjectProjectionExtensions
{
    extension(IQueryable<ProjectProjection> query)
    {
        // The tenant filter is always the first one applied, matching the leading column of every
        // index on this document.
        public IQueryable<ProjectProjection> WithOrganizationId(OrganizationId organizationId) =>
            query.Where(p => p.OrganizationId == organizationId.Value);

        public IQueryable<ProjectProjection> WithSearch(string? search)
        {
            if (string.IsNullOrWhiteSpace(search))
                return query;

            var term = search.Trim();

            return query.Where(p =>
                p.Name.Contains(term, StringComparison.OrdinalIgnoreCase)
                || p.CompanyName.Contains(term, StringComparison.OrdinalIgnoreCase)
            );
        }

        public IQueryable<ProjectProjection> WithFilters(ProjectQuery filters)
        {
            if (filters.Statuses is { Count: > 0 })
                query = query.Where(p => filters.Statuses.Contains(p.Status));

            if (filters.CompanyId is not null)
                query = query.Where(p => p.CompanyId == filters.CompanyId);

            if (filters.TeamId is not null)
                query = query.Where(p => p.TeamId == filters.TeamId);

            if (!string.IsNullOrWhiteSpace(filters.Country))
            {
                var country = filters.Country.Trim().ToUpperInvariant();
                query = query.Where(p => p.WorkCountry == country);
            }

            return query;
        }
    }
}
