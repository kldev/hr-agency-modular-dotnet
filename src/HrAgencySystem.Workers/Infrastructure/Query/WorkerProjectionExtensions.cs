using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.Workers.Application.Port;
using HrAgencySystem.Workers.Projections;

namespace HrAgencySystem.Workers.Infrastructure.Query;

internal static class WorkerProjectionExtensions
{
    extension(IQueryable<WorkerProjection> query)
    {
        // The tenant filter is always the first one applied, matching the leading column of every
        // index on this document.
        public IQueryable<WorkerProjection> WithOrganizationId(OrganizationId organizationId) =>
            query.Where(w => w.OrganizationId == organizationId.Value);

        public IQueryable<WorkerProjection> WithSearch(string? search)
        {
            if (string.IsNullOrWhiteSpace(search))
                return query;

            var term = search.Trim();

            // Names only. Searching by document number would put it in a query string, a browser
            // history and a server log, which is three places it does not belong.
            return query.Where(w => w.FullName.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        public IQueryable<WorkerProjection> WithFilters(WorkerQuery filters)
        {
            if (filters.Statuses is { Count: > 0 })
                query = query.Where(w => filters.Statuses.Contains(w.Status));

            if (filters.Departments is { Count: > 0 })
                query = query.Where(w => filters.Departments.Contains(w.Department));

            if (!string.IsNullOrWhiteSpace(filters.Citizenship))
            {
                var citizenship = filters.Citizenship.Trim().ToUpperInvariant();
                query = query.Where(w => w.Citizenship == citizenship);
            }

            // The two halves of the office. Include is the plain case; exclude is what the other
            // half needs, and it deliberately keeps people who are not on a project at all - they
            // are somebody's to look after, and it is not the one watching the other country.
            if (filters.WorkCountries is { Count: > 0 })
            {
                var countries = Normalize(filters.WorkCountries);
                query = query.Where(w =>
                    w.CurrentWorkCountry != null && countries.Contains(w.CurrentWorkCountry)
                );
            }

            if (filters.ExcludeWorkCountries is { Count: > 0 })
            {
                var excluded = Normalize(filters.ExcludeWorkCountries);
                query = query.Where(w =>
                    w.CurrentWorkCountry == null || !excluded.Contains(w.CurrentWorkCountry)
                );
            }

            return query;
        }
    }

    private static List<string> Normalize(IReadOnlyList<string> countries) =>
        [
            .. countries
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Select(c => c.Trim().ToUpperInvariant()),
        ];
}
