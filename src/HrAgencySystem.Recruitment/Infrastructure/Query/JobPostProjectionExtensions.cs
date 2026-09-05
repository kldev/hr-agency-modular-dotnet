using HrAgencySystem.Recruitment.Domain.JobPostings;
using HrAgencySystem.Recruitment.Projections;

namespace HrAgencySystem.Recruitment.Infrastructure.Query;

internal static class JobPostProjectionExtensions
{
    internal static IQueryable<JobPostProjection> WithOrganizationId(this IQueryable<JobPostProjection> query, Guid organizationId)
    {
        return query.Where(q => q.OrgId == organizationId);
    }

    internal static IQueryable<JobPostProjection> WithCompanyId(this IQueryable<JobPostProjection> query,
        Guid? companyId)
    {
        if (!companyId.HasValue || companyId.Value == Guid.Empty) return query;
        
        return query.Where(q => q.CompanyId == companyId);
    }

    internal static IQueryable<JobPostProjection> WithRecruiterId(
        this IQueryable<JobPostProjection> query, Guid? recruiterId)
    {
        if (!recruiterId.HasValue || recruiterId.Value == Guid.Empty) return query;
        return query.Where(q=>q.RecruiterId == recruiterId);
    }

    internal static IQueryable<JobPostProjection> WithSearch(
        this IQueryable<JobPostProjection> query, string search)
    {
        if (string.IsNullOrWhiteSpace(search)) return query;
        var querySearch = search.Trim();

        return query.Where(q => (q.Title.Contains(querySearch, StringComparison.OrdinalIgnoreCase)
                                || q.Description.Contains(querySearch, StringComparison.OrdinalIgnoreCase)
                                || q.Company.Name.Contains(querySearch, StringComparison.OrdinalIgnoreCase)
                                || q.Company.TaxId.Contains(querySearch, StringComparison.OrdinalIgnoreCase)
                                || q.SearchText.Contains(querySearch, StringComparison.OrdinalIgnoreCase)));
    }
    
    internal static IQueryable<JobPostProjection> WithStatuses(this IQueryable<JobPostProjection> query, IReadOnlyList<JobPostStatus> statuses)
    {
        return statuses.Count == 0 ? query : query.Where(u => statuses.Contains(u.Status));
    }

    internal static IQueryable<JobPostProjection> WithLanguages(this IQueryable<JobPostProjection> query,
        IReadOnlyList<string> languages)
    {
        var upperCaseLanguages = languages.Select(language => language.ToUpperInvariant()).ToList();
        
        return upperCaseLanguages.Count == 0 ? query : query.Where(u => upperCaseLanguages.Contains(u.LanguageCode));
    }
    
    internal static IQueryable<JobPostProjection> WithPostId(this IQueryable<JobPostProjection> query, Guid postId)
    {
        return query.Where(q => q.Id == postId);
    }
}