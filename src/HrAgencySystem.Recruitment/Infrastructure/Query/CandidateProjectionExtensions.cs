using HrAgencySystem.Recruitment.Domain.Candidates;
using HrAgencySystem.Recruitment.Projections;

namespace HrAgencySystem.Recruitment.Infrastructure.Query;

internal static class CandidateProjectionExtensions
{
    extension(IQueryable<CandidateProjection> query)
    {
        internal IQueryable<CandidateProjection> WithCandidateId(Guid candidateId)
        {
            return query.Where(q => q.Id == candidateId);
        }

        internal IQueryable<CandidateProjection> WithOrganizationId(Guid organizationId)
        {
            return query.Where(q => q.OrgId == organizationId);
        }

        internal IQueryable<CandidateProjection> WithCompanyId(Guid? companyId)
        {
            if (!companyId.HasValue || companyId.Value == Guid.Empty) return query;
        
            return query.Where(q => q.CompanyIds.Contains(companyId.Value));
        }

        internal IQueryable<CandidateProjection> WithStatus(CandidateStatus? status)
        {
            return !status.HasValue ? query : query.Where(q => q.Status == status.Value);
        }

        internal IQueryable<CandidateProjection> WithSources(IReadOnlyList<CandidateSource> sources)
        {
            return sources.Count == 0 ? query : query.Where(q => sources.Contains(q.Source));
        }

        internal IQueryable<CandidateProjection> WithTags(IReadOnlyList<Guid> tags)
        {
            if (tags.Count == 0)
                return query;

            return tags.Aggregate(query, (current, tag) => current.Where(q => q.TagsIds.Contains(tag)));
        }

        internal IQueryable<CandidateProjection> WithSearch(string search)
        {
            return  string.IsNullOrWhiteSpace(search) ? query : query.Where(q => 
                q.Email.Contains(search, StringComparison.OrdinalIgnoreCase)
                || q.FirstName.Contains(search, StringComparison.OrdinalIgnoreCase)
                || q.LastName.Contains(search, StringComparison.OrdinalIgnoreCase)
                || q.PhoneNumber.Contains(search, StringComparison.OrdinalIgnoreCase)
            );
        }
    }
}