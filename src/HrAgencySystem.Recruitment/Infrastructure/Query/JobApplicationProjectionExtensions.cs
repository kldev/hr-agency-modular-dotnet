using HrAgencySystem.Recruitment.Domain.Applications;
using HrAgencySystem.Recruitment.Domain.Candidates;
using HrAgencySystem.Recruitment.Projections;

namespace HrAgencySystem.Recruitment.Infrastructure.Query;


internal static class JobApplicationProjectionExtensions
{
    internal static IQueryable<JobApplicationProjection> WithJobApplicationId(this IQueryable<JobApplicationProjection> query, Guid jobApplicationId)
    {
        return query.Where(q => q.Id == jobApplicationId);
    }

    internal static IQueryable<JobApplicationProjection> WithOrganizationId(this IQueryable<JobApplicationProjection> query,
        Guid organizationId)
    {
        return query.Where(q => q.OrgId == organizationId);
    }
    
    internal static IQueryable<JobApplicationProjection> WithCompanyId(this IQueryable<JobApplicationProjection> query,
        Guid? companyId)
    {
        if (!companyId.HasValue || companyId.Value == Guid.Empty) return query;
        
        return query.Where(q => q.CompanyId == companyId.Value);
    }

    internal static IQueryable<JobApplicationProjection> WithStatus(this IQueryable<JobApplicationProjection> query,
        IReadOnlyList<JobApplicationStatus> statuses)
    {
        return statuses.Count == 0 ? query : query.Where(q => statuses.Contains(q.Status));
    }

    internal static IQueryable<JobApplicationProjection> WithSources(this IQueryable<JobApplicationProjection> query,
        IReadOnlyList<CandidateSource> sources)
    {
        return sources.Count == 0 ? query : query.Where(q => sources.Contains(q.Source));
    }

    internal static IQueryable<JobApplicationProjection> WithTags(this IQueryable<JobApplicationProjection> query,
        IReadOnlyList<Guid> tags)
    {
        if (tags.Count == 0)
            return query;

        return tags.Aggregate(query, (current, tag) => current.Where(q => q.TagsIds.Contains(tag)));
    }
    
    internal static IQueryable<JobApplicationProjection> WithSearch(this IQueryable<JobApplicationProjection> query, string search)
    {
        return  string.IsNullOrWhiteSpace(search) ? query : query.Where(q => 
            q.ApplicantFullName.Contains(search, StringComparison.OrdinalIgnoreCase)
            || q.ApplicantEmail.Contains(search, StringComparison.OrdinalIgnoreCase )
            || q.JobPostTitle.Contains(search, StringComparison.OrdinalIgnoreCase )
            || q.Company.Name.Contains(search, StringComparison.OrdinalIgnoreCase )
            );
    }
}