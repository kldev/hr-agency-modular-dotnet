using HrAgencySystem.JobDescription.Domain;
using HrAgencySystem.JobDescription.Projections;

namespace HrAgencySystem.JobDescription.Infrastructure.Query;

internal static class JobDescriptionProjectionExtensions
{
    internal static IQueryable<JobDescriptionProjection> WithOrganizationId(this IQueryable<JobDescriptionProjection> query, Guid organizationId)
    {
        return query.Where(q => q.OrgId == organizationId);
    }

    internal static IQueryable<JobDescriptionProjection> WithCompanyId(this IQueryable<JobDescriptionProjection> query,
        Guid? companyId)
    {
        if (!companyId.HasValue || companyId.Value == Guid.Empty) return query;
        
        return query.Where(q => q.CompanyId == companyId);
    }

    internal static IQueryable<JobDescriptionProjection> WithRecruiterId(
        this IQueryable<JobDescriptionProjection> query, Guid? recruiterId)
    {
        if (!recruiterId.HasValue || recruiterId.Value == Guid.Empty) return query;
        return query.Where(q=>q.RecruiterId == recruiterId);
    }

    internal static IQueryable<JobDescriptionProjection> WithSearch(
        this IQueryable<JobDescriptionProjection> query, string search)
    {
        if (string.IsNullOrWhiteSpace(search)) return query;
        var querySearch = search.Trim();

        return query.Where(q => q.Title.Contains(querySearch, StringComparison.OrdinalIgnoreCase)
                                || q.Description.Contains(querySearch, StringComparison.OrdinalIgnoreCase)
                                || q.Company.Name.Contains(querySearch, StringComparison.OrdinalIgnoreCase)
                                || q.Company.TaxId.Contains(querySearch, StringComparison.OrdinalIgnoreCase));
    }
    
    internal static IQueryable<JobDescriptionProjection> WithStatuses(this IQueryable<JobDescriptionProjection> query, IReadOnlyList<JobDescriptionStatus> statuses)
    {
        return statuses.Count == 0 ? query : query.Where(u => statuses.Contains(u.Status));
    }
    
    internal static IQueryable<JobDescriptionProjection> WithJobDescriptionId(this IQueryable<JobDescriptionProjection> query, Guid jobDescriptionId)
    {
        return query.Where(q => q.Id == jobDescriptionId);
    }
}