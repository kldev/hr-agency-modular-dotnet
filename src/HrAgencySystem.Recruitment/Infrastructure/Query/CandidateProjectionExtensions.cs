using HrAgencySystem.Recruitment.Domain.Candidate;
using HrAgencySystem.Recruitment.Projections;

namespace HrAgencySystem.Recruitment.Infrastructure.Query;

internal static class CandidateProjectionExtensions
{
    internal static IQueryable<CandidateProjection> WithCandidateId(this IQueryable<CandidateProjection> query, Guid candidateId)
    {
        return query.Where(q => q.Id == candidateId);
    }

    internal static IQueryable<CandidateProjection> WithOrganizationId(this IQueryable<CandidateProjection> query,
        Guid organizationId)
    {
        return query.Where(q => q.OrgId == organizationId);
    }
    
    internal static IQueryable<CandidateProjection> WithCompanyId(this IQueryable<CandidateProjection> query,
        Guid? companyId)
    {
        if (!companyId.HasValue || companyId.Value == Guid.Empty) return query;
        
        return query.Where(q => q.CompanyIds.Contains(companyId.Value));
    }
    
    internal static IQueryable<CandidateProjection> WithStatus(this IQueryable<CandidateProjection> query,
        CandidateStatus? status)
    {
        return !status.HasValue ? query : query.Where(q => q.Status == status.Value);
    }

    internal static IQueryable<CandidateProjection> WithSources(this IQueryable<CandidateProjection> query,
        IReadOnlyList<CandidateSource> sources)
    {
        return sources.Count == 0 ? query : query.Where(q => sources.Contains(q.Source));
    }

    internal static IQueryable<CandidateProjection> WithTags(this IQueryable<CandidateProjection> query,
        IReadOnlyList<Guid> tags)
    {
        if (tags.Count == 0)
            return query;

        return tags.Aggregate(query, (current, tag) => current.Where(q => q.TagsIds.Contains(tag)));
    }
    
    internal static IQueryable<CandidateProjection> WithSearch(this IQueryable<CandidateProjection> query, string search)
    {
        return  string.IsNullOrWhiteSpace(search) ? query : query.Where(q => 
            q.Email.Contains(search, StringComparison.OrdinalIgnoreCase)
            || q.FirstName.Contains(search, StringComparison.OrdinalIgnoreCase)
            || q.LastName.Contains(search, StringComparison.OrdinalIgnoreCase)
            || q.PhoneNumber.Contains(search, StringComparison.OrdinalIgnoreCase)
            );
    }
}