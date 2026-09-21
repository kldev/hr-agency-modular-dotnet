using HrAgencySystem.Projects.Application.Port;
using HrAgencySystem.Projects.Projections;
using HrAgencySystem.SharedKernel.Tenant;

namespace HrAgencySystem.Projects.Infrastructure.Query;

internal static class PositionProjectionExtensions
{
    extension(IQueryable<ProjectPositionProjection> query)
    {
        public IQueryable<ProjectPositionProjection> WithOrganizationId(
            OrganizationId organizationId
        ) => query.Where(p => p.OrganizationId == organizationId.Value);

        /// <summary>
        /// Both names are searched: somebody looking for "Painter" should find "Painter Belgium",
        /// and somebody who only knows what the contract says should find it too.
        /// </summary>
        public IQueryable<ProjectPositionProjection> WithSearch(string? search)
        {
            if (string.IsNullOrWhiteSpace(search))
                return query;

            var term = search.Trim();

            return query.Where(p =>
                p.Name.Contains(term, StringComparison.OrdinalIgnoreCase)
                || p.ContractName.Contains(term, StringComparison.OrdinalIgnoreCase)
            );
        }

        public IQueryable<ProjectPositionProjection> WithFilters(PositionQuery filters)
        {
            if (filters.ProjectId is not null)
                query = query.Where(p => p.ProjectId == filters.ProjectId);

            if (!filters.IncludeArchived)
                query = query.Where(p => !p.IsArchived);

            if (filters.ContractType is not null)
                query = query.Where(p => p.ContractType == filters.ContractType);

            return query;
        }
    }
}
