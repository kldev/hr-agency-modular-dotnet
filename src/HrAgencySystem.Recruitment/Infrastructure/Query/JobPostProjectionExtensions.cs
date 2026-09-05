using HrAgencySystem.Recruitment.Domain.JobPostings;
using HrAgencySystem.Recruitment.Projections;

namespace HrAgencySystem.Recruitment.Infrastructure.Query;

internal static class JobPostProjectionExtensions
{
    extension(IQueryable<JobPostProjection> query)
    {
        internal IQueryable<JobPostProjection> WithOrganizationId(Guid organizationId)
        {
            return query.Where(q => q.OrgId == organizationId);
        }

        internal IQueryable<JobPostProjection> WithCompanyId(Guid? companyId)
        {
            if (!companyId.HasValue || companyId.Value == Guid.Empty) return query;
        
            return query.Where(q => q.CompanyId == companyId);
        }

        internal IQueryable<JobPostProjection> WithRecruiterId(Guid? recruiterId)
        {
            if (!recruiterId.HasValue || recruiterId.Value == Guid.Empty) return query;
            return query.Where(q=>q.RecruiterId == recruiterId);
        }

        internal IQueryable<JobPostProjection> WithSearch(string search)
        {
            if (string.IsNullOrWhiteSpace(search)) return query;
            var querySearch = search.Trim();

            return query.Where(q => q.Title.Contains(querySearch, StringComparison.OrdinalIgnoreCase)
                                    || q.Description.Contains(querySearch, StringComparison.OrdinalIgnoreCase)
                                    || q.Company.Name.Contains(querySearch, StringComparison.OrdinalIgnoreCase)
                                    || q.Company.TaxId.Contains(querySearch, StringComparison.OrdinalIgnoreCase)
                                    || q.SearchText.Contains(querySearch, StringComparison.OrdinalIgnoreCase));
        }

        internal IQueryable<JobPostProjection> WithStatuses(IReadOnlyList<JobPostStatus> statuses)
        {
            return statuses.Count == 0 ? query : query.Where(u => statuses.Contains(u.Status));
        }

        internal IQueryable<JobPostProjection> WithLanguages(IReadOnlyList<string> languages)
        {
            var upperCaseLanguages = languages.Select(language => language.ToUpperInvariant()).ToList();
        
            return upperCaseLanguages.Count == 0 ? query : query.Where(u => upperCaseLanguages.Contains(u.LanguageCode));
        }

        internal IQueryable<JobPostProjection> WithPostId(Guid postId)
        {
            return query.Where(q => q.Id == postId);
        }
    }
}