using HrAgencySystem.SharedKernel.Tenant;
using HrAgencySystem.Workers.Application.Port;
using HrAgencySystem.Workers.Projections;

namespace HrAgencySystem.Workers.Infrastructure.Query;

internal static class AssignmentProjectionExtensions
{
    extension(IQueryable<AssignmentProjection> query)
    {
        public IQueryable<AssignmentProjection> WithOrganizationId(OrganizationId organizationId) =>
            query.Where(a => a.OrganizationId == organizationId.Value);

        public IQueryable<AssignmentProjection> WithSearch(string? search)
        {
            if (string.IsNullOrWhiteSpace(search))
                return query;

            var term = search.Trim();

            return query.Where(a =>
                a.WorkerFullName.Contains(term, StringComparison.OrdinalIgnoreCase)
                || a.ProjectName.Contains(term, StringComparison.OrdinalIgnoreCase)
                || a.PositionName.Contains(term, StringComparison.OrdinalIgnoreCase)
            );
        }

        public IQueryable<AssignmentProjection> WithFilters(AssignmentQuery filters)
        {
            if (filters.Statuses is { Count: > 0 })
                query = query.Where(a => filters.Statuses.Contains(a.Status));

            if (filters.WorkerId is not null)
                query = query.Where(a => a.WorkerId == filters.WorkerId);

            if (filters.ProjectId is not null)
                query = query.Where(a => a.ProjectId == filters.ProjectId);

            if (filters.PositionId is not null)
                query = query.Where(a => a.PositionId == filters.PositionId);

            if (!string.IsNullOrWhiteSpace(filters.WorkCountry))
            {
                var country = filters.WorkCountry.Trim().ToUpperInvariant();
                query = query.Where(a => a.WorkCountry == country);
            }

            return query;
        }
    }
}
